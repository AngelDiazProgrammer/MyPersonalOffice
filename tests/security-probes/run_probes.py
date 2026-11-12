"""Sondeos de seguridad reproducibles contra una instancia en marcha de la aplicación (spec 003, T-014).

Uso:   python tests/security-probes/run_probes.py [http://localhost:5080]
Requisito: la aplicación debe estar corriendo (dotnet run --project src/ATLAS.Web).
No llama a ningún modelo (no gasta cupo ni dinero). Termina con código 1 si alguna comprobación falla.
"""
import base64, http.client, json, os, socket, sys, urllib.parse, uuid, zipfile, io

BASE = sys.argv[1] if len(sys.argv) > 1 else 'http://localhost:5080'
URL = urllib.parse.urlparse(BASE)
HOST, PORT = URL.hostname, URL.port or 80
CLIENT = {'X-Office-Client': 'probes'}
EVIL = {'Origin': 'http://evil.example'}
results = []


def call(method, path, body=None, headers=None, timeout=15):
    h = dict(headers or {})
    data = body
    if isinstance(body, (dict, list)):
        data = json.dumps(body).encode('utf-8'); h.setdefault('Content-Type', 'application/json')
    c = http.client.HTTPConnection(HOST, PORT, timeout=timeout)
    try:
        c.request(method, path, body=data, headers=h)
        r = c.getresponse()
        return r.status, {k.lower(): v for k, v in r.getheaders()}, r.read()
    except Exception as e:  # noqa: BLE001
        return 'ERR', {}, repr(e).encode()
    finally:
        c.close()


def check(name, ok, detail=''):
    results.append(ok)
    print(('PASS' if ok else 'FAIL'), '·', name, ('' if ok else f'→ {detail}'))


def multipart(filename, content):
    bd = '----probe' + uuid.uuid4().hex
    body = (f'--{bd}\r\nContent-Disposition: form-data; name="file"; filename="{filename}"\r\nContent-Type: application/pdf\r\n\r\n').encode() + content + f'\r\n--{bd}--\r\n'.encode()
    return body, {'Content-Type': f'multipart/form-data; boundary={bd}', **CLIENT}


# ---------------------------------------------------------------- 1. Cabeceras
s, h, _ = call('GET', '/')
check('CSP presente y sin unsafe-eval', 'content-security-policy' in h and 'unsafe-eval' not in h['content-security-policy'], h.get('content-security-policy'))
check('X-Content-Type-Options: nosniff', h.get('x-content-type-options') == 'nosniff')
check('Referrer-Policy: no-referrer', h.get('referrer-policy') == 'no-referrer')
check('X-Frame-Options presente', 'x-frame-options' in h)

# ---------------------------------------------------------------- 2. Origen y CSRF
s, *_ = call('GET', '/api/office/state', headers=EVIL)
check('GET con origen ajeno → 403', s == 403, s)
for path in ('/api/workspaces/reset', '/api/agents/reload', '/api/knowledge/reload'):
    s, *_ = call('POST', path, headers=EVIL)
    check(f'POST {path} con origen ajeno → 403', s == 403, s)
s, *_ = call('POST', '/api/workspaces/reset')
check('POST sin cabecera de cliente → 403', s == 403, s)
s, *_ = call('POST', '/api/workspaces/reset', headers=CLIENT)
check('POST con cabecera de cliente → 200', s == 200, s)
s, *_ = call('GET', '/api/office/state', headers={'Host': 'evil.com'})
check('Host ajeno → 400', s == 400, s)

# ---------------------------------------------------------------- 3. WebSocket de SignalR
s, _, b = call('POST', '/hubs/office/negotiate?negotiateVersion=1', headers=CLIENT)
token = json.loads(b)['connectionToken'] if s == 200 else None
check('negotiate propio → 200', token is not None, s)
for origin, expected in (('http://evil.example', '403'), (BASE, '101')):
    if not token: break
    sock = socket.create_connection((HOST, PORT), timeout=8)
    key = base64.b64encode(os.urandom(16)).decode()
    sock.sendall((f'GET /hubs/office?id={token} HTTP/1.1\r\nHost: {HOST}:{PORT}\r\nOrigin: {origin}\r\nUpgrade: websocket\r\nConnection: Upgrade\r\n'
                  f'Sec-WebSocket-Key: {key}\r\nSec-WebSocket-Version: 13\r\n\r\n').encode())
    line = sock.recv(256).decode(errors='replace').splitlines()[0]
    sock.close()
    check(f'WebSocket con Origin {origin} → {expected}', f' {expected} ' in line, line)

# ---------------------------------------------------------------- 4. Archivos y rutas
for p in ('/.env', '/configuration/appsettings.json', '/agents/orchestrator.md', '/api/documents/generated/..%2F..%2Fappsettings.json', '/api/documents/generated/not-a-guid'):
    s, *_ = call('GET', p)
    check(f'GET {p} no expone archivos', s in (404, 400), s)

# ---------------------------------------------------------------- 5. Validación y subida
call('POST', '/api/workspaces/select', {'id': 'base-conocimiento'}, CLIENT)
for label, body, expected in (
        ('mensaje vacío', {'message': ''}, 400), ('9000 caracteres', {'message': 'a' * 9000}, 400),
        ('4 documentos', {'message': 'x', 'documentIds': [str(uuid.uuid4()) for _ in range(4)]}, 400)):
    s, *_ = call('POST', '/api/chat', body, CLIENT)
    check(f'chat: {label} → {expected}', s == expected, s)
for label, name, content in (('texto como .pdf', 'a.pdf', b'no soy un pdf'), ('ejecutable', 'c.exe', b'MZ' + b'\0' * 50),
                             ('doble extensión', 'x.pdf.exe', b'%PDF-1.4 x'), ('vacío', 'e.pdf', b''), ('PDF corrupto', 'b.pdf', b'%PDF-1.4\n' + b'\0\xff' * 100)):
    b, h = multipart(name, content)
    s, *_ = call('POST', '/api/documents', b, h)
    check(f'subida rechazada: {label}', s == 400, s)
b, h = multipart('big.pdf', b'%PDF-1.4\n' + b'A' * (13 * 1024 * 1024))
s, *_ = call('POST', '/api/documents', b, h, timeout=60)
check('subida de 13 MB rechazada', s in (400, 413), s)

# ---------------------------------------------------------------- 6. Exportación con contenido hostil
hostile = '# T\n\n[x](javascript:alert(1))\n\n![](http://evil.example/p.png)\n\n<script>alert(1)</script>\n'
s, _, b = call('POST', '/api/documents/word', {'title': '../../x\x00<b>', 'markdown': hostile}, CLIENT)
check('exportar Word hostil → 200', s == 200, s)
if s == 200:
    doc_id = json.loads(b)['id']
    _, _, raw = call('GET', f'/api/documents/generated/{doc_id}')
    z = zipfile.ZipFile(io.BytesIO(raw))
    rels = z.read('word/_rels/document.xml.rels').decode() if 'word/_rels/document.xml.rels' in z.namelist() else ''
    xml = z.read('word/document.xml').decode()
    check('Word sin enlace javascript:', 'javascript:' not in rels)
    check('Word sin etiqueta <script>', '<script' not in xml)
s, *_ = call('POST', '/api/documents/pdf', {'title': 'CON', 'markdown': hostile, 'style': 'apa'}, CLIENT)
check('exportar PDF hostil → 200', s == 200, s)
s, *_ = call('POST', '/api/documents/pdf', {'title': 't', 'markdown': '# x\n' * 200000}, CLIENT)
check('Markdown gigante rechazado', s == 400, s)

# ---------------------------------------------------------------- 7. Información
s, _, b = call('GET', '/api/llm/status')
txt = b.decode('utf-8', 'replace')
check('estado de modelos sin claves ni rutas locales', s == 200 and 'AIza' not in txt and 'sk-ant' not in txt and 'C:\\\\' not in txt)

call('POST', '/api/workspaces/reset', headers=CLIENT)
failed = results.count(False)
print(f'\n{len(results) - failed}/{len(results)} comprobaciones correctas')
sys.exit(1 if failed else 0)
