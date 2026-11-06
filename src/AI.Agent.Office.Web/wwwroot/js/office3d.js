// Oficina 3D (WebGL con three.js). SOLO PRESENTACIÓN: nunca decide el estado del negocio. Los personajes
// caminan, se sientan, imprimen, se entregan trabajo y celebran como consecuencia visual de los eventos reales
// que llegan desde SignalR.
import * as THREE from '../lib/three/three.module.min.js';

// #region Constantes y configuración de la escena

const U = 1.5;                   // world units per grid tile
const COLS = 16, ROWS = 13;
const WALL_H = 5.2;
const SPEED = 2.3;               // tiles per second
const SEAT_DZ = 0.45;            // seated characters scoot in towards the desk

// Puestos de trabajo (el escritorio cubre x..x+1 en la fila y; la silla está en (x, y-1)). El puesto 0 es del
// Orquestador.
const SLOTS = [
    { x: 4, y: 2 }, { x: 1, y: 2 }, { x: 7, y: 2 }, { x: 10, y: 2 },
    { x: 1, y: 6 }, { x: 4, y: 6 }, { x: 7, y: 6 }, { x: 10, y: 6 },
    { x: 1, y: 10 }, { x: 4, y: 10 }, { x: 7, y: 10 }, { x: 10, y: 10 }
];
const BOOKSHELF_SPOT = { x: 1, y: 3 };   // in front of the bookshelf: where the knowledge-base researcher goes to "consult"
const WHITEBOARD = { x: 14, y: 1 };
const PRINTER_SPOT = { x: 13, y: 1 };
const COFFEE_SPOT = { x: 14, y: 2 };
const SOCIAL = [{ x: 13, y: 5 }, { x: 13, y: 6 }, { x: 14, y: 4 }, { x: 15, y: 4 }, { x: 14, y: 7 }, { x: 15, y: 7 }];
// Símbolos flotantes que salen de un escritorio mientras ese rol trabaja (nadie se levanta de su puesto para
// trabajar).
const ROLE_FX = {
    'kb-researcher': ['📚', '🔎', '📖'], 'incident-responder': ['🛡️', '📞', '📝'], 'compliance-auditor': ['✔', '🔍', '📋'],
    'word-writer': ['✍️', '📄', '¶'], 'pdf-reader': ['📖', '📄', '🔎'], architect: ['📐', '🧩', '▢'],
    'qa-analyst': ['🐞', '✔', '🧪'], teacher: ['🎓', '💡', '➕'], orchestrator: ['📋', '🧭', '💬'], developer: ['{ }', '</>', '01', ';']
};
const CELEBRATE_SPOTS = [{ x: 8, y: 4 }, { x: 8, y: 5 }, { x: 7, y: 4 }, { x: 9, y: 4 }];
const CHAT = ['☕ ¿Un café?', '💬 ¿Cómo va todo?', '😄 Buen trabajo', '📊 ¿Viste las métricas?', '🍪 ¿Galleta?', '🎧 ¿Qué escuchas?'];

const COLORS = {
    idle: '#64748b', thinking: '#a78bfa', working: '#38bdf8', reading: '#34d399', coding: '#22d3ee',
    reviewing: '#fb7185', waiting: '#fbbf24', completed: '#4ade80', error: '#f87171'
};
const LABELS = {
    idle: 'En reposo', thinking: 'Pensando', working: 'Trabajando', reading: 'Leyendo', coding: 'Programando',
    reviewing: 'Revisando', waiting: 'Esperando', completed: 'Completado', error: 'Error'
};
const BUBBLES = { thinking: '💭', working: '⚙️', reading: '📖', coding: '⌨️', reviewing: '🔍', waiting: '⏳', completed: '✅', error: '⚠️' };

// Aspecto de cada agente conocido (los nuevos reciben un aspecto determinista a partir de su id).
const TRAITS = {
    orchestrator: { hair: 'slick', hairColor: 0x1b1b2b, skin: 0xf1c9a5, outfit: 'suit', accessory: 'pin' },
    developer: { hair: 'messy', hairColor: 0x3b2a1a, skin: 0xe0ac84, outfit: 'hoodie', accessory: 'headphones' },
    'pdf-reader': { hair: 'bob', hairColor: 0x6b3b1f, skin: 0xf5d0b0, outfit: 'cardigan', accessory: 'glasses' },
    teacher: { hair: 'bun', hairColor: 0x8a8a96, skin: 0xd9a577, outfit: 'vest', accessory: 'glasses' },
    architect: { hair: 'short', hairColor: 0x1f1a14, skin: 0xc68e63, outfit: 'jacket', accessory: 'cap' },
    'qa-analyst': { hair: 'ponytail', hairColor: 0xb5651d, skin: 0xf1c9a5, outfit: 'labcoat', accessory: 'badge' },
    'word-writer': { hair: 'curly', hairColor: 0x20140e, skin: 0x8d5a3a, outfit: 'scarf', accessory: 'pen' },
    'kb-researcher': { hair: 'bob', hairColor: 0x2b1b12, skin: 0xe0ac84, outfit: 'cardigan', accessory: 'glasses' },
    'incident-responder': { hair: 'short', hairColor: 0x1b1b2b, skin: 0xf1c9a5, outfit: 'jacket', accessory: 'badge' },
    'format-editor': { hair: 'ponytail', hairColor: 0x6b3b1f, skin: 0xd9a577, outfit: 'vest', accessory: 'pen' },
    'compliance-auditor': { hair: 'slick', hairColor: 0x3b3b46, skin: 0xc68e63, outfit: 'suit', accessory: 'glasses' }
};
const HAIRS = ['short', 'messy', 'bob', 'bun', 'ponytail', 'curly', 'slick'];
const OUTFITS = ['hoodie', 'cardigan', 'vest', 'jacket', 'scarf', 'suit'];
const ACCESSORIES = ['glasses', 'headphones', 'cap', 'badge', 'pen', 'none'];
const SKINS = [0xf1c9a5, 0xe0ac84, 0xc68e63, 0x8d5a3a, 0xf5d0b0, 0xd9a577];
const HAIR_COLORS = [0x1b1b2b, 0x3b2a1a, 0x6b3b1f, 0xb5651d, 0x20140e, 0xd8b45a];

// Aspecto (peinado, ropa, accesorios, piel) de un agente: el definido en TRAITS o uno determinista a partir de
// su id.
function traitsFor(id, index) {
    if (TRAITS[id]) return TRAITS[id];
    let h = 0; for (const ch of id) h = (h * 31 + ch.charCodeAt(0)) >>> 0;
    const pick = list => list[(h + index * 7) % list.length];
    return { hair: pick(HAIRS), hairColor: pick(HAIR_COLORS), skin: SKINS[(h >> 3) % SKINS.length], outfit: pick(OUTFITS), accessory: ACCESSORIES[(h >> 5) % ACCESSORIES.length] };
}

const blocked = new Set();
const block = (x, y) => blocked.add(x + ',' + y);
SLOTS.forEach(s => { block(s.x, s.y); block(s.x + 1, s.y); });
[[14, 5], [15, 5], [14, 6], [15, 6], [15, 2], [0, 1], [0, 12], [15, 12], [0, 4], [0, 5], [0, 2], [0, 3], [13, 0]].forEach(p => block(p[0], p[1]));

// #endregion

// #region Estado del módulo

let renderer, scene, camera, canvas, tip, raf = 0, last = 0, now = 0, resizeObs = null, dotnet = null;
const cam = { yaw: 0.78, pitch: 0.62, zoom: 1, ty: 0.78, tp: 0.62, tz: 1, dragging: false, lastX: 0, lastY: 0, moved: 0 };
const baseTarget = new THREE.Vector3(COLS * U / 2, 0.9, ROWS * U / 2 + 0.4);
const target = baseTarget.clone();
const chars = new Map();
const desks = new Map();
const pulses = [];
const particles = [];
const raycaster = new THREE.Raycaster();
const pointer = new THREE.Vector2();
const listeners = [];
let socialBusy = false, selectedId = null;
let rosterKey = '', orchActive = false, restUntil = 0;           // a request is being processed / quiet time after the celebration
const participants = new Set();                  // agents that took part in the current request (they get congratulated)
let whiteboard = null, holo = null, tv = null, printer = null, coffee = null, clockHands = null;
let tvDirty = true, lastTvLine = 'Oficina lista';

// #endregion

// #region Utilidades de dibujo (three.js)

const mat = (color, o = {}) => new THREE.MeshStandardMaterial({ color, roughness: 0.62, metalness: 0.08, ...o });
const glow = (color, intensity = 1.2) => new THREE.MeshStandardMaterial({ color, emissive: color, emissiveIntensity: intensity, roughness: 0.4 });
const lerp = (a, b, t) => a + (b - a) * t;
const angleDiff = (a, b) => Math.atan2(Math.sin(b - a), Math.cos(b - a));
const rnd = (a, b) => a + Math.random() * (b - a);

// Crea una caja con material y sombra y la agrega al padre.
function box(parent, w, h, d, material, x, y, z, shadow = true) {
    const m = new THREE.Mesh(new THREE.BoxGeometry(w, h, d), material);
    m.position.set(x, y, z); m.castShadow = shadow; m.receiveShadow = true;
    parent.add(m); return m;
}
// Crea un cilindro.
function cyl(parent, rt, rb, h, material, x, y, z, seg = 20) {
    const m = new THREE.Mesh(new THREE.CylinderGeometry(rt, rb, h, seg), material);
    m.position.set(x, y, z); m.castShadow = true; m.receiveShadow = true;
    parent.add(m); return m;
}
// Crea una esfera (o elipsoide con los factores de escala).
function sphere(parent, r, material, x, y, z, sx = 1, sy = 1, sz = 1, seg = 14) {
    const m = new THREE.Mesh(new THREE.SphereGeometry(r, seg, seg - 2), material);
    m.position.set(x, y, z); m.scale.set(sx, sy, sz); m.castShadow = true;
    parent.add(m); return m;
}
// Crea una textura dibujando en un canvas 2D.
function canvasTex(w, h, draw) {
    const c = document.createElement('canvas'); c.width = w; c.height = h;
    const g = c.getContext('2d'); draw(g, w, h);
    const t = new THREE.CanvasTexture(c); t.colorSpace = THREE.SRGBColorSpace; t.anisotropy = 4;
    return { canvas: c, ctx: g, tex: t };
}

// #endregion

// #region Búsqueda de caminos en la cuadrícula

// True si la casilla está dentro de la oficina y no está bloqueada.
function isFree(x, y, extra) {
    return x >= 0 && y >= 0 && x < COLS && y < ROWS && !blocked.has(x + ',' + y) && !(extra && extra.has(x + ',' + y));
}
// Camino más corto (búsqueda en anchura) entre dos casillas, esquivando muebles y casillas extra bloqueadas;
// null si no hay.
function findPath(sx, sy, tx, ty, extra) {
    if (sx === tx && sy === ty) return [];
    const key = (x, y) => x + ',' + y;
    const prev = new Map([[key(sx, sy), null]]);
    const queue = [[sx, sy]];
    while (queue.length) {
        const [cx, cy] = queue.shift();
        for (const [dx, dy] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) {
            const nx = cx + dx, ny = cy + dy, k = key(nx, ny);
            if (prev.has(k)) continue;
            if (nx < 0 || ny < 0 || nx >= COLS || ny >= ROWS) continue;
            if (!(nx === tx && ny === ty) && !isFree(nx, ny, extra)) continue;
            prev.set(k, [cx, cy]);
            if (nx === tx && ny === ty) {
                const path = [];
                let cur = [nx, ny];
                while (cur && !(cur[0] === sx && cur[1] === sy)) { path.unshift({ x: cur[0], y: cur[1] }); cur = prev.get(key(cur[0], cur[1])); }
                return path;
            }
            queue.push([nx, ny]);
        }
    }
    return null;
}

const texCache = new Map();
// #endregion

// #region Partículas y efectos

// Textura con un emoji o símbolo, para partículas y etiquetas.
function glyphTexture(text, color = '#ffffff') {
    const key = text + color;
    if (texCache.has(key)) return texCache.get(key);
    const { tex } = canvasTex(128, 128, g => {
        g.clearRect(0, 0, 128, 128); g.textAlign = 'center'; g.textBaseline = 'middle';
        g.font = '84px Segoe UI Emoji, Consolas, sans-serif'; g.fillStyle = color; g.shadowColor = color; g.shadowBlur = 12;
        g.fillText(text, 64, 70);
    });
    texCache.set(key, tex); return tex;
}
// Textura circular difusa para brillos y vapor.
function softTexture() {
    if (texCache.has('soft')) return texCache.get('soft');
    const { tex } = canvasTex(64, 64, g => {
        const gr = g.createRadialGradient(32, 32, 2, 32, 32, 30);
        gr.addColorStop(0, 'rgba(255,255,255,.9)'); gr.addColorStop(1, 'rgba(255,255,255,0)');
        g.fillStyle = gr; g.fillRect(0, 0, 64, 64);
    });
    texCache.set('soft', tex); return tex;
}

// Crea una partícula (sprite) con velocidad, vida y tamaño.
function spawn(opts) {
    if (particles.length > 220) return;
    const mat2 = new THREE.SpriteMaterial({
        map: opts.map, color: opts.color ?? 0xffffff, transparent: true, depthWrite: false,
        blending: opts.additive ? THREE.AdditiveBlending : THREE.NormalBlending
    });
    const s = new THREE.Sprite(mat2);
    s.position.copy(opts.pos); s.scale.setScalar(opts.size ?? 0.3);
    scene.add(s);
    particles.push({ s, v: opts.vel ?? new THREE.Vector3(0, 0.6, 0), g: opts.gravity ?? 0, life: opts.life ?? 1.5, max: opts.life ?? 1.5, grow: opts.grow ?? 0, size: opts.size ?? 0.3, spin: opts.spin ?? 0 });
}

// Mueve y desvanece las partículas; elimina las que terminaron.
function updateParticles(dt) {
    for (let i = particles.length - 1; i >= 0; i--) {
        const p = particles[i];
        p.life -= dt;
        if (p.life <= 0) { scene.remove(p.s); p.s.material.dispose(); particles.splice(i, 1); continue; }
        p.v.y -= p.g * dt;
        p.s.position.addScaledVector(p.v, dt);
        const k = p.life / p.max;
        p.s.material.opacity = Math.min(1, k * 2);
        p.s.scale.setScalar(p.size * (1 + p.grow * (1 - k)));
        if (p.spin) p.s.material.rotation += p.spin * dt;
    }
}

// Lanza confeti desde un punto (para celebrar).
function confetti(x, z, count = 60) {
    const colors = [0xfbbf24, 0xa78bfa, 0x38bdf8, 0x4ade80, 0xfb7185, 0xf472b6];
    for (let i = 0; i < count; i++) {
        spawn({
            map: null, color: colors[i % colors.length], pos: new THREE.Vector3(x, 2.2, z), size: rnd(0.1, 0.2),
            vel: new THREE.Vector3(rnd(-2.5, 2.5), rnd(2.5, 5.5), rnd(-2.5, 2.5)), gravity: 7, life: rnd(1.8, 3), spin: rnd(-6, 6)
        });
    }
}
// Lanza un puñado de emojis que suben y se desvanecen.
function sparkles(pos, emoji = '✨', count = 8) {
    for (let i = 0; i < count; i++)
        spawn({ map: glyphTexture(emoji), pos: pos.clone(), size: rnd(0.25, 0.45), vel: new THREE.Vector3(rnd(-1, 1), rnd(0.8, 2), rnd(-1, 1)), life: rnd(0.8, 1.4) });
}

// #endregion

// #region Construcción del mundo 3D (oficina y muebles)

// Construye toda la escena: luces, piso, paredes, escritorios y muebles.
function buildWorld() {
    scene = new THREE.Scene();
    scene.background = new THREE.Color(0x070b1d);
    scene.fog = new THREE.Fog(0x070b1d, 45, 100);

    scene.add(new THREE.HemisphereLight(0x9aa8ff, 0x1a1040, 0.8));
    const sun = new THREE.DirectionalLight(0xdfe3ff, 1.4);
    sun.position.set(28, 28, 22);
    sun.castShadow = true;
    sun.shadow.mapSize.set(2048, 2048);
    Object.assign(sun.shadow.camera, { left: -26, right: 26, top: 26, bottom: -26, near: 1, far: 90 });
    sun.shadow.camera.updateProjectionMatrix();
    sun.shadow.bias = -0.0004; sun.shadow.normalBias = 0.02;
    sun.target.position.set(COLS * U / 2, 0, ROWS * U / 2);
    scene.add(sun, sun.target);
    const violet = new THREE.PointLight(0x8b5cf6, 16, 30); violet.position.set(4, 4.2, 3); scene.add(violet);
    const blue = new THREE.PointLight(0x38bdf8, 12, 30); blue.position.set(21, 4.2, 10); scene.add(blue);

    // Piso.
    const floorTex = canvasTex(256, 256, (g, w, h) => {
        for (let j = 0; j < 2; j++) for (let i = 0; i < 2; i++) {
            g.fillStyle = (i + j) % 2 ? '#262c66' : '#2e3478';
            g.fillRect(i * w / 2, j * h / 2, w / 2, h / 2);
        }
        g.strokeStyle = 'rgba(139,120,255,.35)'; g.lineWidth = 3;
        g.strokeRect(1, 1, w - 2, h - 2); g.beginPath(); g.moveTo(w / 2, 0); g.lineTo(w / 2, h); g.moveTo(0, h / 2); g.lineTo(w, h / 2); g.stroke();
    });
    floorTex.tex.wrapS = floorTex.tex.wrapT = THREE.RepeatWrapping;
    floorTex.tex.repeat.set(COLS / 2, ROWS / 2);
    const floor = new THREE.Mesh(new THREE.PlaneGeometry(COLS * U, ROWS * U),
        new THREE.MeshStandardMaterial({ map: floorTex.tex, roughness: 0.34, metalness: 0.32 }));
    floor.rotation.x = -Math.PI / 2; floor.position.set(COLS * U / 2, 0, ROWS * U / 2); floor.receiveShadow = true;
    scene.add(floor);
    box(scene, COLS * U + 1.2, 0.5, ROWS * U + 1.2, mat(0x10143a), COLS * U / 2 + 0.3, -0.27, ROWS * U / 2 + 0.3, false);

    // Alfombra del pasillo principal.
    const rug = canvasTex(512, 96, (g, w, h) => {
        g.fillStyle = '#4c2a9a'; g.fillRect(0, 0, w, h);
        g.strokeStyle = '#c4b5fd'; g.lineWidth = 5; g.strokeRect(8, 8, w - 16, h - 16);
        g.strokeStyle = 'rgba(196,181,253,.35)'; g.lineWidth = 2; g.strokeRect(18, 18, w - 36, h - 36);
        g.fillStyle = 'rgba(196,181,253,.25)';
        for (let x = 40; x < w - 30; x += 38) { g.beginPath(); g.moveTo(x, h / 2 - 12); g.lineTo(x + 12, h / 2); g.lineTo(x, h / 2 + 12); g.lineTo(x - 12, h / 2); g.fill(); }
    });
    const rugMesh = new THREE.Mesh(new THREE.PlaneGeometry(12.5 * U, 1.7 * U), new THREE.MeshStandardMaterial({ map: rug.tex, roughness: 0.95 }));
    rugMesh.rotation.x = -Math.PI / 2; rugMesh.position.set(7.4 * U, 0.015, 4 * U); rugMesh.receiveShadow = true;
    scene.add(rugMesh);

    buildWalls();
    SLOTS.forEach((s, i) => buildDesk(s, i));
    buildFurniture();
}

// Textura de la ciudad nocturna que se ve por las ventanas.
function skylineTexture() {
    return canvasTex(512, 256, (g, w, h) => {
        const sky = g.createLinearGradient(0, 0, 0, h);
        sky.addColorStop(0, '#1b1450'); sky.addColorStop(0.6, '#5b2a8f'); sky.addColorStop(1, '#f472b6');
        g.fillStyle = sky; g.fillRect(0, 0, w, h);
        for (let i = 0; i < 60; i++) { g.fillStyle = 'rgba(255,255,255,.7)'; g.fillRect(Math.random() * w, Math.random() * h * 0.5, 2, 2); }
        g.fillStyle = '#fde68a'; g.beginPath(); g.arc(w * 0.8, h * 0.22, 16, 0, 7); g.fill();
        let x = 0;
        while (x < w) {
            const bw = 28 + Math.random() * 40, bh = 60 + Math.random() * 120;
            g.fillStyle = '#0d0a2b'; g.fillRect(x, h - bh, bw, bh);
            for (let wy = h - bh + 8; wy < h - 6; wy += 12)
                for (let wx = x + 5; wx < x + bw - 6; wx += 10)
                    if (Math.random() > 0.45) { g.fillStyle = Math.random() > 0.5 ? '#fde68a' : '#7dd3fc'; g.fillRect(wx, wy, 5, 6); }
            x += bw + 4;
        }
    });
}

// Paredes con ventanas, pizarra, televisor, reloj y afiches.
function buildWalls() {
    const wallMat = mat(0x2a3070, { roughness: 0.85 });
    box(scene, 0.3, WALL_H, ROWS * U, wallMat, -0.15, WALL_H / 2, ROWS * U / 2, false);
    box(scene, COLS * U, WALL_H, 0.3, wallMat, COLS * U / 2, WALL_H / 2, -0.15, false);
    const trim = mat(0x4a50a8);
    box(scene, 0.12, 0.25, ROWS * U, trim, 0.06, 0.125, ROWS * U / 2, false);
    box(scene, COLS * U, 0.25, 0.12, trim, COLS * U / 2, 0.125, 0.06, false);
    const cornice = glow(0x7c3aed, 1.6);
    box(scene, 0.08, 0.08, ROWS * U, cornice, 0.05, WALL_H - 0.15, ROWS * U / 2, false);
    box(scene, COLS * U, 0.08, 0.08, cornice, COLS * U / 2, WALL_H - 0.15, 0.05, false);

    const sky = skylineTexture();
    const frame = mat(0x6d74d6);
    const winMat = new THREE.MeshBasicMaterial({ map: sky.tex });
    const addWindow = (cx, cz, alongX) => {
        const g = new THREE.Group();
        g.add(new THREE.Mesh(new THREE.BoxGeometry(3.5, 2.5, 0.12), frame));
        const pane = new THREE.Mesh(new THREE.PlaneGeometry(3.2, 2.2), winMat); pane.position.z = 0.07; g.add(pane);
        const bar = new THREE.Mesh(new THREE.BoxGeometry(0.06, 2.2, 0.05), frame); bar.position.z = 0.08; g.add(bar);
        g.position.set(cx, 2.8, cz);
        if (!alongX) g.rotation.y = Math.PI / 2;
        scene.add(g);
    };
    [2.6, 6.4, 10.2, 14.0].forEach(x => addWindow(x, 0.02, true));
    [2.8, 7.2, 11.6, 16.0].forEach(z => addWindow(0.02, z, false));

    const sign = canvasTex(1024, 128, (g, w, h) => {
        g.clearRect(0, 0, w, h);
        g.font = '800 78px Segoe UI, sans-serif'; g.textAlign = 'center'; g.textBaseline = 'middle';
        g.shadowColor = '#8b5cf6'; g.shadowBlur = 28; g.fillStyle = '#e9e1ff'; g.fillText('AI AGENT OFFICE', w / 2, h / 2);
        g.shadowBlur = 10; g.fillText('AI AGENT OFFICE', w / 2, h / 2);
    });
    const signMesh = new THREE.Mesh(new THREE.PlaneGeometry(6.4, 0.8), new THREE.MeshBasicMaterial({ map: sign.tex, transparent: true }));
    signMesh.position.set(9.4, 4.55, 0.03); scene.add(signMesh);

    // Pizarra.
    whiteboard = canvasTex(720, 440, () => { });
    whiteboard.busy = false;
    const wbMesh = new THREE.Mesh(new THREE.PlaneGeometry(3.8, 2.2), new THREE.MeshBasicMaterial({ map: whiteboard.tex }));
    wbMesh.position.set(21.8, 2.9, 0.03); scene.add(wbMesh);
    box(scene, 4.0, 2.4, 0.08, mat(0x9aa3c7), 21.8, 2.9, 0.0, false);
    drawWhiteboard(0);

    // Televisor con el estado en vivo.
    tv = canvasTex(512, 320, () => { });
    const tvMesh = new THREE.Mesh(new THREE.PlaneGeometry(2.3, 1.44), new THREE.MeshBasicMaterial({ map: tv.tex }));
    tvMesh.position.set(17.5, 3.1, 0.06); scene.add(tvMesh);
    box(scene, 2.45, 1.58, 0.08, mat(0x0a0d24), 17.5, 3.1, 0.02, false);

    // Reloj de pared (hora real).
    const clock = new THREE.Group(); clock.position.set(0.05, 4.0, 11.2); clock.rotation.y = Math.PI / 2; scene.add(clock);
    const face = new THREE.Mesh(new THREE.CircleGeometry(0.42, 32), new THREE.MeshBasicMaterial({ color: 0xf1f5f9 })); clock.add(face);
    const rim = new THREE.Mesh(new THREE.TorusGeometry(0.42, 0.04, 8, 32), mat(0x4a50a8)); clock.add(rim);
    const hand = (len, w, color) => { const m = new THREE.Mesh(new THREE.BoxGeometry(w, len, 0.02), new THREE.MeshBasicMaterial({ color })); m.position.z = 0.02; const p = new THREE.Group(); m.position.y = len / 2; p.add(m); clock.add(p); return p; };
    clockHands = { h: hand(0.22, 0.04, 0x1e1b4b), m: hand(0.32, 0.03, 0x1e1b4b), s: hand(0.34, 0.012, 0xf43f5e) };

    // Afiches.
    [[0.04, 2.6, 2.2, 0x7c3aed, '</>'], [0.04, 2.6, 5.2, 0x0ea5e9, '📊']].forEach(([x, y, z, c, glyph]) => {
        const p = canvasTex(128, 176, g => { g.fillStyle = '#' + c.toString(16).padStart(6, '0'); g.fillRect(0, 0, 128, 176); g.fillStyle = 'rgba(255,255,255,.9)'; g.font = '56px Segoe UI Emoji, Consolas'; g.textAlign = 'center'; g.fillText(glyph, 64, 98); });
        const m = new THREE.Mesh(new THREE.PlaneGeometry(0.9, 1.25), new THREE.MeshBasicMaterial({ map: p.tex })); m.position.set(x, y, z); m.rotation.y = Math.PI / 2; scene.add(m);
    });
}

// Dibuja la pizarra (cambia cuando el Orquestador está planificando).
function drawWhiteboard(t) {
    const { ctx: g, canvas: c, tex } = whiteboard;
    g.fillStyle = '#f1f5f9'; g.fillRect(0, 0, c.width, c.height);
    g.strokeStyle = '#cbd5e1'; g.lineWidth = 6; g.strokeRect(3, 3, c.width - 6, c.height - 6);
    g.fillStyle = '#7c3aed'; g.font = '700 36px Segoe UI, sans-serif'; g.fillText('Plan del día', 40, 64);
    g.lineWidth = 7; g.lineCap = 'round';
    ['#7c3aed', '#0ea5e9', '#10b981', '#f59e0b'].forEach((col, i) => {
        const k = whiteboard.busy ? (0.35 + 0.65 * ((Math.sin(t * 1.5 + i) + 1) / 2)) : 1;
        g.strokeStyle = col;
        g.beginPath(); g.moveTo(40, 130 + i * 68); g.lineTo(40 + (460 - i * 40) * k, 130 + i * 68); g.stroke();
        g.fillStyle = col; g.beginPath(); g.arc(580 + i * 36, 140 + i * 68, 12, 0, 7); g.fill();
    });
    tex.needsUpdate = true;
}

// Dibuja el televisor con el último estado de la oficina.
function drawTv() {
    const { ctx: g, canvas: c, tex } = tv;
    g.fillStyle = '#070b1d'; g.fillRect(0, 0, c.width, c.height);
    g.fillStyle = '#1b1450'; g.fillRect(0, 0, c.width, 44);
    g.fillStyle = '#c4b5fd'; g.font = '700 24px Segoe UI, sans-serif'; g.textAlign = 'left'; g.fillText('ESTADO DE LA OFICINA', 16, 31);
    let y = 76;
    [...chars.values()].forEach(a => {
        g.fillStyle = COLORS[a.status]; g.beginPath(); g.arc(24, y - 8, 7, 0, 7); g.fill();
        g.fillStyle = '#e6e9f8'; g.font = '600 22px Segoe UI, sans-serif'; g.fillText(a.name, 42, y);
        g.fillStyle = COLORS[a.status]; g.font = '20px Segoe UI, sans-serif'; g.textAlign = 'right'; g.fillText(LABELS[a.status], c.width - 16, y); g.textAlign = 'left';
        y += 30;
    });
    g.fillStyle = '#8f97bd'; g.font = '18px Segoe UI, sans-serif';
    const line = lastTvLine.length > 44 ? lastTvLine.slice(0, 43) + '…' : lastTvLine;
    g.fillText(line, 16, c.height - 14);
    tex.needsUpdate = true;
}

// Construye un puesto de trabajo: escritorio, pantalla, teclado, silla y luces de estado.
function buildDesk(slot, index) {
    const g = new THREE.Group();
    g.position.set(slot.x * U, 0, slot.y * U);
    scene.add(g);

    const top = mat(0x4a4fa8, { roughness: 0.4, metalness: 0.25 });
    const dark = mat(0x1d2150);
    box(g, 2.95, 0.08, 1.3, top, 1.5, 1.0, 0.75);
    [[0.12, 0.2], [2.88, 0.2], [0.12, 1.3], [2.88, 1.3]].forEach(([lx, lz]) => box(g, 0.08, 1.0, 0.08, dark, lx, 0.5, lz));
    box(g, 2.8, 0.55, 0.05, dark, 1.5, 0.72, 1.36);
    box(g, 0.9, 0.9, 1.15, dark, 2.45, 0.5, 0.75);
    const led = glow(0x8b5cf6, 1.8);
    box(g, 2.9, 0.05, 0.04, led, 1.5, 0.98, 1.4, false);

    // Teclado (brilla al teclear), taza, lámpara de escritorio y planta pequeña.
    const keyMat = new THREE.MeshStandardMaterial({ color: 0x0d1030, emissive: 0x000000, roughness: 0.5 });
    box(g, 0.8, 0.03, 0.28, keyMat, 0.75, 1.06, 0.3);
    cyl(g, 0.09, 0.09, 0.16, mat(0xe2e8f0), 2.4, 1.12, 0.45, 14);
    cyl(g, 0.12, 0.14, 0.04, mat(0x2a2f5c), 0.2, 1.06, 0.2, 14);
    box(g, 0.03, 0.4, 0.03, mat(0x9aa3c7), 0.2, 1.26, 0.2); sphere(g, 0.07, glow(0xfde68a, 1.4), 0.2, 1.5, 0.28);

    const mon = new THREE.Group(); mon.position.set(1.95, 1.04, 0.62); mon.rotation.y = 0.7; g.add(mon);
    cyl(mon, 0.28, 0.32, 0.04, mat(0x1d2150), 0, 0.02, 0, 20);
    box(mon, 0.07, 0.32, 0.07, mat(0x2a2f5c), 0, 0.2, 0);
    box(mon, 1.32, 0.82, 0.07, mat(0x0a0d24), 0, 0.75, 0);
    const screen = canvasTex(192, 116, () => { });
    const screenMesh = new THREE.Mesh(new THREE.PlaneGeometry(1.2, 0.7), new THREE.MeshBasicMaterial({ map: screen.tex }));
    screenMesh.position.set(0, 0.75, 0.04); mon.add(screenMesh);
    const bezelGlow = new THREE.Mesh(new THREE.BoxGeometry(1.36, 0.86, 0.02), glow(0x8b5cf6, 0.8)); bezelGlow.position.set(0, 0.75, -0.04); mon.add(bezelGlow);

    // Silla.
    const chair = new THREE.Group(); chair.position.set(0.75, 0, -0.75 + SEAT_DZ); g.add(chair);
    box(chair, 0.78, 0.1, 0.74, mat(0x1b1f45, { roughness: 0.7 }), 0, 0.58, 0);
    box(chair, 0.74, 0.95, 0.1, mat(0x2a2f6b), 0, 1.1, -0.36);
    cyl(chair, 0.05, 0.05, 0.5, mat(0x555aa0), 0, 0.3, 0, 10);
    cyl(chair, 0.38, 0.38, 0.05, mat(0x14173a), 0, 0.05, 0, 5);

    // Sombra suave de contacto y resplandor del estado sobre el piso.
    const contact = new THREE.Mesh(new THREE.PlaneGeometry(3.6, 2.2), new THREE.MeshBasicMaterial({ map: shadowTexture(), transparent: true, depthWrite: false }));
    contact.rotation.x = -Math.PI / 2; contact.position.set(1.5, 0.025, 0.9); g.add(contact);
    const decal = new THREE.Mesh(new THREE.CircleGeometry(2.4, 40),
        new THREE.MeshBasicMaterial({ color: 0x8b5cf6, transparent: true, opacity: 0, blending: THREE.AdditiveBlending, depthWrite: false }));
    decal.rotation.x = -Math.PI / 2; decal.position.set(1.5, 0.03, 0.4); decal.scale.set(1.3, 0.9, 1); g.add(decal);
    const light = new THREE.PointLight(0x8b5cf6, 0, 7); light.position.set(1.5, 2.4, 0.3); g.add(light);

    desks.set(index, { slot, group: g, led, keyMat, screen, decal, light, bezelGlow, chair, lastDraw: 0 });
}

// Textura de sombra suave de contacto.
function shadowTexture() {
    if (texCache.has('shadow')) return texCache.get('shadow');
    const { tex } = canvasTex(128, 80, g => {
        const gr = g.createRadialGradient(64, 40, 6, 64, 40, 60);
        gr.addColorStop(0, 'rgba(0,0,0,.5)'); gr.addColorStop(1, 'rgba(0,0,0,0)');
        g.fillStyle = gr; g.fillRect(0, 0, 128, 80);
    });
    texCache.set('shadow', tex); return tex;
}

// Muebles compartidos: mesa de reuniones, cafetera, impresora, estantería y sofá.
function buildFurniture() {
    // Mesa de reuniones con un anillo holográfico.
    const tbl = new THREE.Group(); tbl.position.set(15 * U, 0, 6 * U); scene.add(tbl);
    cyl(tbl, 1.35, 1.35, 0.08, mat(0x5b3fb0, { roughness: 0.35, metalness: 0.3 }), 0, 0.98, 0, 40);
    cyl(tbl, 0.2, 0.35, 0.95, mat(0x2a2f5c), 0, 0.48, 0, 16);
    cyl(tbl, 0.6, 0.6, 0.04, mat(0x1d2150), 0, 0.02, 0, 24);
    holo = new THREE.Mesh(new THREE.TorusGeometry(0.55, 0.025, 10, 48), new THREE.MeshBasicMaterial({ color: 0x67e8f9, transparent: true, opacity: 0.85 }));
    holo.position.set(0, 1.6, 0); holo.rotation.x = Math.PI / 2; tbl.add(holo);
    const core = new THREE.Mesh(new THREE.SphereGeometry(0.2, 16, 12), new THREE.MeshBasicMaterial({ color: 0xa78bfa, transparent: true, opacity: 0.55, wireframe: true }));
    core.position.set(0, 1.6, 0); tbl.add(core); holo.userData.core = core;
    [[-1.9, 0], [1.9, 0], [0, -1.9], [0, 1.9]].forEach(([sx, sz]) => { cyl(tbl, 0.32, 0.32, 0.08, mat(0x22275a), sx, 0.5, sz, 16); cyl(tbl, 0.05, 0.05, 0.5, mat(0x555aa0), sx, 0.25, sz, 8); });

    // Cafetera.
    const cs = new THREE.Group(); cs.position.set(15.5 * U, 0, 2.5 * U); scene.add(cs);
    box(cs, 1.3, 1.0, 1.1, mat(0x2d3270), 0, 0.5, 0);
    box(cs, 0.8, 0.95, 0.7, mat(0x14173a), 0, 1.5, 0);
    const coffeeLed = glow(0x4ade80, 2); box(cs, 0.18, 0.18, 0.05, coffeeLed, 0.2, 1.7, 0.36, false);
    cyl(cs, 0.12, 0.1, 0.22, mat(0xe2e8f0), -0.15, 1.1, 0.1, 12);
    coffee = { pos: new THREE.Vector3(15.5 * U - 0.15, 1.35, 2.5 * U + 0.1), led: coffeeLed, busyUntil: 0 };

    // Impresora en la pared del fondo.
    const pr = new THREE.Group(); pr.position.set(13.5 * U, 0, 0.55 * U); scene.add(pr);
    box(pr, 1.3, 0.8, 0.9, mat(0x2d3270), 0, 0.4, 0);                          // stand
    box(pr, 1.1, 0.4, 0.8, mat(0x9aa3c7), 0, 1.0, 0);                          // body
    box(pr, 0.9, 0.05, 0.5, mat(0xf1f5f9), 0, 1.23, -0.05);                    // input tray
    const paper = box(pr, 0.6, 0.015, 0.5, mat(0xffffff, { emissive: 0x222233 }), 0, 0.92, 0.55, false); paper.visible = false;
    const printerLed = glow(0x38bdf8, 2); box(pr, 0.08, 0.05, 0.03, printerLed, 0.4, 1.12, 0.42, false);
    printer = { paper, led: printerLed, pos: new THREE.Vector3(13.5 * U, 1.0, 0.55 * U + 0.6), busyUntil: 0, base: pr.position.clone() };

    // Estantería contra la pared izquierda.
    const bs = new THREE.Group(); bs.position.set(0.4, 0, 3 * U); scene.add(bs);
    box(bs, 0.6, 2.7, 3.0, mat(0x3b3f86), 0, 1.35, 0);
    const bookColors = [0xf87171, 0xfbbf24, 0x4ade80, 0x38bdf8, 0xa78bfa, 0xf472b6];
    for (let row = 0; row < 4; row++) {
        box(bs, 0.62, 0.05, 3.0, mat(0x2a2f6b), 0.02, 0.35 + row * 0.62, 0, false);
        let z = -1.35;
        while (z < 1.2) {
            const w = rnd(0.12, 0.26), h = rnd(0.35, 0.55);
            box(bs, 0.34, h, w, mat(bookColors[Math.floor(Math.random() * 6)]), 0.12, 0.4 + row * 0.62 + h / 2 - 0.02, z, false);
            z += w + 0.03;
        }
    }

    // Sofá, mesa de centro y lámpara de pie.
    const sofa = new THREE.Group(); sofa.position.set(0.75, 0, 5 * U); scene.add(sofa);
    box(sofa, 1.1, 0.45, 2.8, mat(0x6d44c9), 0.05, 0.42, 0);
    box(sofa, 0.3, 0.95, 2.8, mat(0x5636a8), -0.42, 0.8, 0);
    [-1.45, 1.45].forEach(z => box(sofa, 1.1, 0.7, 0.25, mat(0x5636a8), 0.05, 0.6, z));
    box(sofa, 0.8, 0.14, 1.1, mat(0x7c5ae0), 0.1, 0.72, -0.62); box(sofa, 0.8, 0.14, 1.1, mat(0x7c5ae0), 0.1, 0.72, 0.62);
    box(sofa, 0.8, 0.06, 1.2, mat(0x3b3f86), 1.5, 0.45, 0); cyl(sofa, 0.05, 0.05, 0.45, mat(0x2a2f5c), 1.5, 0.22, 0, 8);
    cyl(scene, 0.03, 0.03, 2.4, mat(0x9aa3c7), 0.5, 1.2, 8.6, 8);
    const bulb = new THREE.Mesh(new THREE.SphereGeometry(0.2, 16, 12), glow(0xffd79a, 2.2)); bulb.position.set(0.5, 2.5, 8.6); scene.add(bulb);
    const lampLight = new THREE.PointLight(0xffc98a, 6, 9); lampLight.position.set(0.5, 2.4, 8.6); scene.add(lampLight);

    [[0, 1], [0, 12], [15, 12]].forEach(([tx, ty]) => plant((tx + 0.5) * U, (ty + 0.5) * U));
}

// Una planta decorativa.
function plant(x, z) {
    const g = new THREE.Group(); g.position.set(x, 0, z); scene.add(g);
    cyl(g, 0.38, 0.28, 0.6, mat(0x5b3fb0), 0, 0.3, 0, 16);
    const leaf = mat(0x22c55e, { roughness: 0.8 });
    [[0, 1.05, 0, 0.5], [0.25, 1.4, 0.1, 0.38], [-0.22, 1.35, -0.12, 0.36], [0.05, 1.75, 0.05, 0.3]].forEach(([lx, ly, lz, r]) => {
        const m = new THREE.Mesh(new THREE.IcosahedronGeometry(r, 1), leaf); m.position.set(lx, ly, lz); m.castShadow = true; g.add(m);
    });
}

// #endregion

// #region Personajes: modelo, movimiento y vida social

// Construye el cuerpo 3D de un personaje (cabeza, ojos, brazos, piernas, ropa y accesorios) a partir de su
// aspecto.
function makeCharView(c) {
    const t = c.traits;
    const col = new THREE.Color(c.color);
    const dark = col.clone().multiplyScalar(0.55);
    const skin = mat(t.skin, { roughness: 0.75 });
    const hairMat = mat(t.hairColor, { roughness: 0.85 });
    const white = mat(0xf1f5f9, { roughness: 0.7 });

    // Paleta de la ropa.
    let body = mat(col, { roughness: 0.6 }), sleeve = body;
    if (t.outfit === 'suit') { body = mat(0x242a58, { roughness: 0.55 }); sleeve = body; }
    else if (t.outfit === 'vest' || t.outfit === 'labcoat') { sleeve = white; if (t.outfit === 'labcoat') body = white; else body = white; }
    else if (t.outfit === 'jacket') { body = mat(col.clone().multiplyScalar(0.7), { roughness: 0.6 }); sleeve = body; }
    const pants = mat(t.outfit === 'suit' ? 0x1a1e45 : 0x232859);

    const group = new THREE.Group();
    const hips = new THREE.Group(); group.add(hips);

    const torso = new THREE.Mesh(new THREE.CapsuleGeometry(0.2, 0.42, 6, 14), body);
    torso.position.y = 0.38; torso.scale.set(1.15, 1, 0.78); torso.castShadow = true; hips.add(torso);
    box(hips, 0.46, 0.07, 0.3, mat(dark), 0, 0.08, 0, false);                      // belt

    // Detalles de la ropa.
    switch (t.outfit) {
        case 'suit':
            box(hips, 0.2, 0.46, 0.02, white, 0, 0.42, 0.155, false);               // shirt
            box(hips, 0.06, 0.4, 0.025, mat(col), 0, 0.4, 0.17, false);              // tie
            box(hips, 0.1, 0.1, 0.03, mat(col), 0, 0.64, 0.165, false);              // knot
            break;
        case 'hoodie':
            sphere(hips, 0.17, body, 0, 0.7, -0.13, 1.2, 0.8, 0.8);                  // hood
            box(hips, 0.3, 0.14, 0.03, mat(dark), 0, 0.22, 0.165, false);            // pocket
            break;
        case 'cardigan':
            box(hips, 0.14, 0.5, 0.025, white, 0, 0.4, 0.158, false);                // inner shirt
            break;
        case 'vest':
            box(hips, 0.38, 0.46, 0.03, mat(col), 0, 0.4, 0.158, false);             // vest front
            box(hips, 0.06, 0.4, 0.035, white, 0, 0.42, 0.172, false);
            break;
        case 'jacket':
            box(hips, 0.12, 0.1, 0.04, mat(dark), -0.1, 0.66, 0.15, false); box(hips, 0.12, 0.1, 0.04, mat(dark), 0.1, 0.66, 0.15, false);
            box(hips, 0.14, 0.44, 0.02, mat(col), 0, 0.4, 0.16, false);
            break;
        case 'labcoat': {
            const coat = new THREE.Mesh(new THREE.CylinderGeometry(0.26, 0.31, 0.5, 18, 1, true), white);
            coat.position.set(0, -0.14, 0); coat.scale.set(1.15, 1, 0.8); coat.castShadow = true; hips.add(coat);
            box(hips, 0.16, 0.44, 0.02, mat(col), 0, 0.42, 0.158, false);
            break;
        }
        case 'scarf': {
            const scarf = new THREE.Mesh(new THREE.TorusGeometry(0.15, 0.05, 8, 16), mat(0xf59e0b)); scarf.rotation.x = Math.PI / 2; scarf.position.y = 0.7; hips.add(scarf);
            box(hips, 0.08, 0.3, 0.04, mat(0xf59e0b), 0.08, 0.52, 0.17, false);
            break;
        }
    }

    // Cabeza.
    const head = new THREE.Group(); head.position.y = 0.92; hips.add(head);
    const skull = new THREE.Mesh(new THREE.SphereGeometry(0.2, 22, 18), skin); skull.castShadow = true; head.add(skull);
    sphere(head, 0.045, skin, -0.2, -0.01, 0, 0.6, 1, 0.8); sphere(head, 0.045, skin, 0.2, -0.01, 0, 0.6, 1, 0.8);   // ears
    sphere(head, 0.03, mat(t.skin, { roughness: 0.6 }), 0, -0.03, 0.2, 1, 0.9, 1.1);                                  // nose
    const sclera = new THREE.MeshBasicMaterial({ color: 0xffffff }), pupilMat = new THREE.MeshBasicMaterial({ color: 0x1e1b3a });
    const eyes = [-0.075, 0.075].map(ex => {
        const e = new THREE.Group(); e.position.set(ex, 0.03, 0.172); head.add(e);
        const w = new THREE.Mesh(new THREE.SphereGeometry(0.047, 12, 10), sclera); w.scale.set(1, 1.1, 0.6); e.add(w);
        const p = new THREE.Mesh(new THREE.SphereGeometry(0.026, 10, 8), pupilMat); p.position.z = 0.02; e.add(p);
        return { group: e, pupil: p };
    });
    const brows = [-0.075, 0.075].map(bx => box(head, 0.07, 0.016, 0.02, hairMat, bx, 0.1, 0.18, false));
    const mouth = new THREE.Mesh(new THREE.TorusGeometry(0.05, 0.012, 6, 12, Math.PI), new THREE.MeshBasicMaterial({ color: 0x7a3b3b }));
    mouth.position.set(0, -0.085, 0.185); mouth.rotation.z = Math.PI; head.add(mouth);
    const blush = [-0.12, 0.12].map(bx => { const b = sphere(head, 0.03, new THREE.MeshBasicMaterial({ color: 0xf9a8a8, transparent: true, opacity: 0.35 }), bx, -0.05, 0.17, 1, 0.6, 0.3); b.castShadow = false; return b; });
    void blush;

    // Cabello.
    const cap = new THREE.Mesh(new THREE.SphereGeometry(0.215, 20, 14, 0, Math.PI * 2, 0, Math.PI * 0.56), hairMat);
    cap.rotation.x = -0.35; cap.position.set(0, 0.02, -0.01); cap.castShadow = true; head.add(cap);
    switch (t.hair) {
        case 'slick': box(head, 0.3, 0.05, 0.1, hairMat, 0.04, 0.2, 0.1, false); break;
        case 'messy': [[-0.12, 0.22, 0.02], [0.02, 0.26, 0.04], [0.13, 0.2, -0.02], [-0.04, 0.23, -0.12]].forEach(([x, y, z]) => sphere(head, 0.07, hairMat, x, y, z)); break;
        case 'bob': box(head, 0.06, 0.32, 0.3, hairMat, -0.2, -0.08, -0.02); box(head, 0.06, 0.32, 0.3, hairMat, 0.2, -0.08, -0.02); box(head, 0.4, 0.3, 0.07, hairMat, 0, -0.06, -0.19); break;
        case 'bun': sphere(head, 0.09, hairMat, 0, 0.27, -0.1); break;
        case 'ponytail': { const tail = new THREE.Mesh(new THREE.CapsuleGeometry(0.05, 0.3, 4, 8), hairMat); tail.position.set(0, -0.03, -0.25); tail.rotation.x = -0.4; tail.castShadow = true; head.add(tail); break; }
        case 'curly': for (let i = 0; i < 9; i++) { const a = (i / 9) * Math.PI * 2; sphere(head, 0.07, hairMat, Math.cos(a) * 0.19, 0.1 + Math.sin(i) * 0.05, Math.sin(a) * 0.17 - 0.02); } break;
    }

    // Accesorios.
    switch (t.accessory) {
        case 'glasses': {
            const gm = new THREE.MeshStandardMaterial({ color: 0x111827, metalness: 0.4, roughness: 0.4 });
            [-0.075, 0.075].forEach(ex => { const r = new THREE.Mesh(new THREE.TorusGeometry(0.052, 0.008, 6, 16), gm); r.position.set(ex, 0.03, 0.2); head.add(r); });
            box(head, 0.04, 0.01, 0.01, gm, 0, 0.04, 0.2, false);
            break;
        }
        case 'headphones': {
            const hm = mat(0x1f2937, { roughness: 0.4 });
            const band = new THREE.Mesh(new THREE.TorusGeometry(0.22, 0.02, 8, 20, Math.PI), hm); band.rotation.z = 0; band.position.y = 0.02; head.add(band);
            [-0.22, 0.22].forEach(x => { const cup = cyl(head, 0.07, 0.07, 0.05, mat(col), x, 0, 0, 14); cup.rotation.z = Math.PI / 2; });
            break;
        }
        case 'cap': {
            const cm = mat(col); const hat = new THREE.Mesh(new THREE.SphereGeometry(0.225, 18, 12, 0, Math.PI * 2, 0, Math.PI * 0.5), cm); hat.position.y = 0.03; hat.rotation.x = -0.15; head.add(hat);
            box(head, 0.28, 0.02, 0.2, cm, 0, 0.07, 0.26, false);
            break;
        }
        case 'pen': { const pen = cyl(head, 0.01, 0.01, 0.2, mat(0xef4444), 0.2, 0.08, -0.02, 6); pen.rotation.z = 0.5; break; }
        case 'badge': box(hips, 0.08, 0.1, 0.02, mat(0xfacc15), 0.13, 0.5, 0.17, false); break;
        case 'pin': { const pin = new THREE.Mesh(new THREE.OctahedronGeometry(0.05), glow(0xfbbf24, 1.5)); pin.position.set(-0.13, 0.55, 0.175); hips.add(pin); break; }
    }

    // Brazos.
    const makeArm = side => {
        const pivot = new THREE.Group(); pivot.position.set(side * 0.3, 0.62, 0); hips.add(pivot);
        const upper = new THREE.Mesh(new THREE.CapsuleGeometry(0.065, 0.42, 4, 10), sleeve); upper.position.y = -0.27; upper.castShadow = true; pivot.add(upper);
        const hand = new THREE.Mesh(new THREE.SphereGeometry(0.075, 10, 10), skin); hand.position.y = -0.55; pivot.add(hand);
        return { pivot, hand };
    };
    const armL = makeArm(-1), armR = makeArm(1);

    const makeLeg = side => {
        const thigh = new THREE.Group(); thigh.position.set(side * 0.12, 0, 0); hips.add(thigh);
        const th = new THREE.Mesh(new THREE.CapsuleGeometry(0.085, 0.36, 4, 10), pants); th.position.y = -0.27; th.castShadow = true; thigh.add(th);
        const shin = new THREE.Group(); shin.position.y = -0.5; thigh.add(shin);
        const sh = new THREE.Mesh(new THREE.CapsuleGeometry(0.075, 0.36, 4, 10), pants); sh.position.y = -0.24; sh.castShadow = true; shin.add(sh);
        const shoe = new THREE.Mesh(new THREE.BoxGeometry(0.16, 0.08, 0.28), mat(0x0f1230)); shoe.position.set(0, -0.5, 0.05); shoe.castShadow = true; shin.add(shoe);
        return { thigh, shin };
    };
    const legL = makeLeg(-1), legR = makeLeg(1);

    // Objetos en la mano (documento, lupa, taza, celular).
    const carry = new THREE.Mesh(new THREE.BoxGeometry(0.26, 0.34, 0.02), new THREE.MeshStandardMaterial({ color: 0xf8fafc, emissive: 0x334155, roughness: 0.6 }));
    carry.visible = false; hips.add(carry);
    const lens = new THREE.Group();
    lens.add(new THREE.Mesh(new THREE.TorusGeometry(0.1, 0.018, 8, 20), new THREE.MeshStandardMaterial({ color: 0xe2e8f0, metalness: 0.6, roughness: 0.3 })));
    const handle = new THREE.Mesh(new THREE.CylinderGeometry(0.014, 0.014, 0.16, 6), mat(0xe2e8f0)); handle.position.set(-0.08, -0.1, 0); handle.rotation.z = 0.7; lens.add(handle);
    lens.visible = false; hips.add(lens);
    const mug = cyl(hips, 0.06, 0.055, 0.12, mat(0xe2e8f0), 0, 0, 0, 12); mug.visible = false;
    const phone = box(hips, 0.1, 0.18, 0.015, new THREE.MeshStandardMaterial({ color: 0x0b0f24, emissive: 0x1e40af, emissiveIntensity: 0.8 }), 0, 0, 0, false); phone.visible = false;

    const hit = new THREE.Mesh(new THREE.BoxGeometry(0.9, 2.1, 0.9), new THREE.MeshBasicMaterial({ visible: false }));
    hit.position.y = 1.0; group.add(hit);

    // Aro de selección y sombra suave.
    const ring = new THREE.Mesh(new THREE.RingGeometry(0.45, 0.55, 32), new THREE.MeshBasicMaterial({ color: 0xfbbf24, transparent: true, opacity: 0.9, side: THREE.DoubleSide, depthWrite: false }));
    ring.rotation.x = -Math.PI / 2; ring.position.y = 0.04; ring.visible = false; group.add(ring);
    const blob = new THREE.Mesh(new THREE.PlaneGeometry(1.1, 1.1), new THREE.MeshBasicMaterial({ map: shadowTexture(), transparent: true, depthWrite: false }));
    blob.rotation.x = -Math.PI / 2; blob.position.y = 0.03; group.add(blob);

    const label = canvasTex(512, 256, () => { });
    const sprite = new THREE.Sprite(new THREE.SpriteMaterial({ map: label.tex, transparent: true, depthTest: false, depthWrite: false }));
    sprite.scale.set(3.4, 1.7, 1); sprite.center.set(0.5, 0); sprite.renderOrder = 10;
    group.add(sprite);

    return { group, hips, head, eyes, brows, mouth, armL, armR, legL, legR, carry, lens, mug, phone, hit, ring, label, sprite, torso };
}

// Un agente en la escena: posición en la cuadrícula, estado, viajes pendientes (trips) y animaciones.
class Char {
    constructor(meta, slot, slotIndex) {
        this.id = meta.id;
        this.slot = slot; this.slotIndex = slotIndex;
        this.seed = Math.random() * 10;
        this.trips = []; this.trip = null; this.path = []; this.pathGoal = '';
        this.phase = 0; this.dwell = 0; this.say = null; this.waitingFor = [];
        this.status = 'idle'; this.task = null;
        this.nextSocial = now + 12 + Math.random() * 20;
        this.nextMicro = now + 6 + Math.random() * 10;
        this.sitBlend = 1; this.yaw = 0; this.heading = 0; this.moving = false; this.carry = null; this.micro = null; this.look = null;
        this.update(meta);
        this.traits = traitsFor(this.id, slotIndex);
        const seat = this.seat();
        this.x = seat.x + 0.5; this.y = seat.y + 0.5; this.sitting = true;
        this.view = makeCharView(this);
        this.view.hit.userData.char = this;
        scene.add(this.view.group);
        this.labelKey = '';
    }
    update(meta) {
        this.name = meta.name; this.color = meta.color || '#8b5cf6'; this.icon = meta.icon || '🤖';
        this.isOrch = !!meta.isOrchestrator; this.role = meta.role || '';
    }
    seat() { return { x: this.slot.x, y: this.slot.y - 1 }; }
    approach() { return { x: this.slot.x + 1, y: this.slot.y - 1 }; }
    tile() { return { x: Math.floor(this.x), y: Math.floor(this.y) }; }
    setStatus(status, task) {
        if (status !== this.status) { this.statusSince = now; tvDirty = true; if (status !== 'idle') lastTvLine = `${this.name}: ${LABELS[status]}`; }
        this.status = status; this.task = task || null;
        if (status === 'idle') this.nextSocial = now + 15 + Math.random() * 25;
    }
    speak(text, seconds = 3.2) { this.say = { text, until: now + seconds }; }
    startMicro(type, dur) { this.micro = { type, t0: now, dur }; }
    worldPos() { return new THREE.Vector3(this.x * U, 0, this.y * U); }
    desiredSpot() {
        if (this.isOrch && (this.status === 'waiting' || this.status === 'reviewing')) return WHITEBOARD;
        return this.seat();
    }
}

// Nadie deambula mientras hay trabajo: la vida social solo empieza cuando TODOS están libres.
// True si todos están libres: sin solicitud en curso, sin entregas pendientes y fuera de la pausa tras celebrar.
function allIdle() {
    if (orchActive || now < restUntil) return false;
    for (const c of chars.values()) {
        if (c.status !== 'idle' && c.status !== 'completed') return false;
        if ((c.trip && !c.trip.social) || c.trips.some(t => !t.social)) return false;
    }
    return true;
}

// Llegó trabajo nuevo: se cancela cualquier paseo de café o charla para que solo se muevan el Orquestador y
// quien entrega.
// Cancela los paseos sociales pendientes de todos los personajes.
function cancelSocial() {
    chars.forEach(c => {
        c.trips = c.trips.filter(t => !t.social);
        if (c.trip && c.trip.social) { c.trip = null; c.dwell = 0; c.carry = null; c.look = null; c.pathGoal = ''; c.social = false; }
    });
    socialBusy = false;
}

// Casilla libre del centro donde el Orquestador celebra.
function celebrateSpot() {
    return CELEBRATE_SPOTS.find(s => isFree(s.x, s.y)) || CELEBRATE_SPOTS[0];
}

// Casillas ocupadas por personajes sentados (para esquivarlas al caminar).
function seatedTiles(except) {
    const set = new Set();
    chars.forEach(c => { if (c !== except && c.sitting) { const t = c.tile(); set.add(t.x + ',' + t.y); } });
    return set;
}

// Avanza un personaje un cuadro: sigue su viaje actual, camina por el camino calculado, espera (dwell) y se
// sienta al volver a su puesto.
function stepChar(c, dt) {
    if (!c.trip && c.trips.length) { c.trip = c.trips.shift(); c.trip.arrived = false; c.carry = c.trip.carry || null; }
    const goal = c.trip ? (typeof c.trip.tile === 'function' ? c.trip.tile() : c.trip.tile) : c.desiredSpot();
    const goalKey = goal.x + ',' + goal.y;
    const here = c.tile();
    const centered = Math.abs(c.x - (goal.x + 0.5)) < 0.06 && Math.abs(c.y - (goal.y + 0.5)) < 0.06;

    if (c.dwell > 0) {
        c.dwell -= dt;
        c.moving = false;
        if (c.look) { const lp = c.look.worldPos(); c.heading = Math.atan2(lp.x - c.x * U, lp.z - c.y * U); }
        if (c.dwell <= 0) {
            if (c.trip?.onLeave) c.trip.onLeave();
            c.trip = null; c.carry = null; c.look = null; c.pathGoal = '';
            if (c.social) { c.social = false; socialBusy = false; }
        }
        return;
    }

    if (!centered) {
        if (c.pathGoal !== goalKey) {
            const p = findPath(here.x, here.y, goal.x, goal.y, seatedTiles(c)) || findPath(here.x, here.y, goal.x, goal.y, null);
            c.path = p || [{ x: goal.x, y: goal.y }];
            c.pathGoal = goalKey;
        }
        c.sitting = false;
        const next = c.path[0];
        if (!next) { c.x = goal.x + 0.5; c.y = goal.y + 0.5; return; }
        if (c.sitBlend > 0.05) { c.moving = false; return; }          // stand up first
        const tx = next.x + 0.5, ty = next.y + 0.5;
        const dx = tx - c.x, dy = ty - c.y, dist = Math.hypot(dx, dy), step = SPEED * dt;
        c.heading = Math.atan2(dx, dy);
        c.moving = true; c.phase += dt * 10;
        if (dist <= step) { c.x = tx; c.y = ty; c.path.shift(); } else { c.x += dx / dist * step; c.y += dy / dist * step; }
        // Separación suave para que los personajes no se atraviesen.
        chars.forEach(o => {
            if (o === c || o.sitting) return;
            const ox = c.x - o.x, oy = c.y - o.y, d = Math.hypot(ox, oy);
            if (d > 0 && d < 0.55) { c.x += (ox / d) * (0.55 - d) * dt * 3; c.y += (oy / d) * (0.55 - d) * dt * 3; }
        });
        return;
    }

    c.moving = false;
    c.x = goal.x + 0.5; c.y = goal.y + 0.5;
    if (c.trip && !c.trip.arrived) {
        c.trip.arrived = true;
        c.dwell = (c.trip.dwell || 800) / 1000;
        if (c.trip.emote) c.speak(c.trip.emote, Math.max(2.5, c.dwell));
        if (c.trip.lookAt) c.look = typeof c.trip.lookAt === 'function' ? c.trip.lookAt() : c.trip.lookAt;
        if (c.trip.micro) c.startMicro(c.trip.micro, c.dwell);
        if (c.trip.onArrive) c.trip.onArrive();
        return;
    }
    if (!c.trip) {
        const seat = c.seat();
        if (goal.x === seat.x && goal.y === seat.y) { c.sitting = true; c.heading = 0; }
        if (c.isOrch && goal === WHITEBOARD) c.heading = Math.PI;
        scheduleIdleLife(c);
    }
}

// Conductas en reposo: paseos de café o charla y pequeñas animaciones en el escritorio.
// En reposo programa micro-animaciones en el escritorio y, solo si todos están libres, un café o una charla.
function scheduleIdleLife(c) {
    if ((c.status !== 'idle' && c.status !== 'completed') || !c.sitting) return;

    if (c.status === 'idle' && !c.micro && now > c.nextMicro) {
        const pick = ['sip', 'stretch', 'phone'][Math.floor(Math.random() * 3)];
        c.startMicro(pick, pick === 'stretch' ? 2.2 : 3.2);
        c.nextMicro = now + rnd(9, 22);
    }

    if (socialBusy || now < c.nextSocial || !allIdle()) return;
    const partners = [...chars.values()].filter(o => o !== c && (o.status === 'idle' || o.status === 'completed') && o.sitting && !o.trip && !o.trips.length);
    const line = () => CHAT[Math.floor(Math.random() * CHAT.length)];

    if (!partners.length || Math.random() < 0.4) {
        // Pausa de café en solitario.
        socialBusy = true; c.social = true;
        c.trips.push({
            tile: COFFEE_SPOT, dwell: 5200, emote: '☕ Un cafecito', micro: 'sip', social: true,
            onArrive: () => { coffee.busyUntil = now + 5; }
        });
        c.nextSocial = now + rnd(25, 55);
        return;
    }

    const other = partners[Math.floor(Math.random() * partners.length)];
    const i = Math.floor(Math.random() * (SOCIAL.length - 1));
    socialBusy = true; c.social = true;
    c.trips.push({ tile: SOCIAL[i], dwell: 6500, emote: line(), lookAt: () => other, social: true });
    other.trips.push({ tile: SOCIAL[i + 1], dwell: 6500, emote: line(), lookAt: () => c, social: true, onArrive: () => setTimeout(() => other.speak(line(), 3), 1500) });
    c.nextSocial = now + rnd(25, 55); other.nextSocial = now + rnd(25, 55);
}

// #endregion

// #region Pose y animación de los personajes

// Calcula la pose del cuerpo cada cuadro: caminar, sentarse, teclear, celebrar, mirar y las micro-animaciones.
function poseChar(c, dt) {
    const v = c.view, t = now;
    c.sitBlend = c.sitting ? Math.min(1, c.sitBlend + dt * 3.2) : Math.max(0, c.sitBlend - dt * 3.2);
    const sb = c.sitBlend, sit = sb > 0.5;
    const micro = c.micro && now - c.micro.t0 < c.micro.dur ? c.micro : null;
    if (c.micro && !micro) c.micro = null;
    const mk = micro ? (now - micro.t0) / micro.dur : 0;            // 0..1 progress
    const mArc = micro ? Math.sin(Math.min(1, mk) * Math.PI) : 0;   // 0 -> 1 -> 0

    let want = c.moving ? c.heading : c.heading;
    if (c.sitting) want = 0;
    if (c.status === 'error' && c.sitting) want += Math.sin(t * 26) * 0.05;
    c.yaw += angleDiff(c.yaw, want) * Math.min(1, dt * 9);
    v.group.rotation.y = c.yaw;
    v.group.position.set(c.x * U, 0, c.y * U + SEAT_DZ * sb);

    const walk = c.moving ? 1 : 0;
    const sw = Math.sin(c.phase) * walk;
    const cheer = micro && micro.type === 'cheer' ? Math.abs(Math.sin(mk * Math.PI * 4)) * 0.18 : 0;
    const bob = c.moving ? Math.abs(Math.sin(c.phase)) * 0.05 : Math.sin(t * 1.8 + c.seed) * 0.008;
    v.hips.position.y = lerp(0.98, 0.64, sb) + bob + cheer;
    v.hips.rotation.x = c.moving ? 0.08 : (micro && micro.type === 'stretch' ? -0.18 * mArc : 0);
    v.hips.rotation.z = c.moving ? Math.sin(c.phase) * 0.03 : 0;

    v.legL.thigh.rotation.x = lerp(sw * 0.75, -Math.PI / 2, sb);
    v.legR.thigh.rotation.x = lerp(-sw * 0.75, -Math.PI / 2, sb);
    v.legL.shin.rotation.x = lerp(Math.max(0, -sw) * 0.9, Math.PI / 2, sb);
    v.legR.shin.rotation.x = lerp(Math.max(0, sw) * 0.9, Math.PI / 2, sb);

    let aL = -sw * 0.7, aR = sw * 0.7, zL = 0, zR = 0;
    v.carry.visible = false; v.lens.visible = false; v.mug.visible = false; v.phone.visible = false;
    if (sit) {
        const typing = c.status === 'working' || c.status === 'coding';
        aL = -1.3 + (typing ? Math.sin(t * 19) * 0.07 : 0);
        aR = -1.3 + (typing ? Math.sin(t * 19 + 2) * 0.07 : 0);
        if (c.status === 'thinking') { aR = -2.55; zR = -0.35; }
        else if (c.status === 'completed') { aR = -3.0 + Math.sin(t * 9) * 0.25; zR = -0.25; }
        else if (c.status === 'waiting') { aR = -1.2 + Math.abs(Math.sin(t * 7)) * 0.12; }
        else if (c.status === 'reading') { aL = -1.45; aR = -1.45; v.carry.visible = true; v.carry.position.set(0, 0.55, 0.42); v.carry.rotation.set(-0.5, 0, 0); }
        else if (c.status === 'reviewing') { aR = -1.7; v.lens.visible = true; v.lens.position.set(0.32 + Math.sin(t * 2) * 0.1, 0.56, 0.5); v.lens.rotation.set(0, 0, 0); }
    }
    if (c.carry && !sit) {
        aR = -1.25; aL = -1.0;
        v.carry.visible = true; v.carry.position.set(0.12, 0.42, 0.36); v.carry.rotation.set(-0.2, 0, 0);
        v.carry.material.color.set(c.carry === '📌' ? 0xfde68a : 0xf8fafc);
    }
    if (micro) {
        switch (micro.type) {
            case 'sip': aR = lerp(aR, -2.15, mArc); zR = -0.1 * mArc; v.mug.visible = mArc > 0.05; v.mug.position.set(0.13, 0.55 + 0.1 * mArc, 0.22); break;
            case 'stretch': aL = lerp(aL, -3.1, mArc); aR = lerp(aR, -3.1, mArc); zL = 0.45 * mArc; zR = -0.45 * mArc; break;
            case 'phone': aR = lerp(aR, -1.7, mArc); v.phone.visible = mArc > 0.05; v.phone.position.set(0.12, 0.5, 0.3); v.phone.rotation.x = -0.5; break;
            case 'cheer': aL = lerp(aL, -3.0, mArc); aR = lerp(aR, -3.0, mArc); zL = 0.3 * mArc; zR = -0.3 * mArc; break;
            case 'highfive': aR = lerp(aR, -2.95, mArc); break;
            case 'clap': aL = lerp(aL, -1.55, mArc); aR = lerp(aR, -1.55, mArc); zL = (0.32 + Math.sin(t * 17) * 0.3) * mArc; zR = -(0.32 + Math.sin(t * 17) * 0.3) * mArc; break;
            case 'wave': aR = lerp(aR, -2.9, mArc); zR = (-0.2 + Math.sin(t * 12) * 0.35) * mArc; break;
            case 'point': aR = lerp(aR, -1.5, mArc); break;
        }
    }
    v.armL.pivot.rotation.set(aL, 0, zL + 0.06);
    v.armR.pivot.rotation.set(aR, 0, zR - 0.06);

    // Cabeza y ojos.
    let hx = 0, hy = 0, hz = 0;
    if (c.status === 'thinking') { hz = Math.sin(t * 1.6) * 0.14; hx = -0.08; }
    else if (c.status === 'waiting') hy = Math.sin(t * 1.4) * 0.5;
    else if (c.status === 'error') hy = Math.sin(t * 22) * 0.25;
    else if (sit && (c.status === 'working' || c.status === 'coding')) { hy = 0.45; hx = 0.1; }
    else if (c.status === 'idle') hy = Math.sin(t * 0.5 + c.seed) * 0.3;
    if (micro && micro.type === 'phone') { hx = 0.4 * mArc; }
    if (micro && micro.type === 'stretch') { hx = -0.3 * mArc; }
    if (c.look && !sit) {
        const lp = c.look.worldPos();
        const toLook = angleDiff(c.yaw, Math.atan2(lp.x - c.x * U, lp.z - c.y * U));
        hy = THREE.MathUtils.clamp(toLook, -0.9, 0.9);
    }
    v.head.rotation.set(hx, hy, hz);
    const blink = Math.sin(t * 0.9 + c.seed * 3) > 0.985 ? 0.1 : 1;
    const happy = c.status === 'completed' || (micro && micro.type === 'cheer');
    v.eyes.forEach(e => {
        e.group.scale.y = happy ? 0.35 : blink;
        e.pupil.position.x = THREE.MathUtils.clamp(hy * 0.02 + (sit && (c.status === 'working' || c.status === 'coding') ? 0.012 : 0), -0.02, 0.02);
        e.pupil.position.y = c.status === 'thinking' ? 0.015 : 0;
    });
    v.brows.forEach((b, i) => { b.rotation.z = (i ? -1 : 1) * (c.status === 'error' ? -0.4 : c.status === 'thinking' ? 0.25 : 0); b.position.y = c.status === 'thinking' ? 0.125 : 0.1; });
    v.mouth.scale.set(1, c.status === 'error' ? -1 : 1, 1);
    v.mouth.position.y = c.status === 'error' ? -0.115 : (happy ? -0.08 : -0.085);
    v.ring.visible = selectedId === c.id;
    if (v.ring.visible) { v.ring.rotation.z += dt; v.ring.material.opacity = 0.6 + 0.4 * Math.sin(t * 5); }

    // Partículas que nacen de la actividad real.
    if (sit && c.status === 'coding' && Math.random() < dt * 3.5) {
        const d = desks.get(c.slotIndex);
        if (d) spawn({ map: glyphTexture(['{ }', '</>', '01', ';'][Math.floor(Math.random() * 4)], '#67e8f9'), pos: new THREE.Vector3(d.group.position.x + 0.75, 1.3, d.group.position.z + 0.3), size: 0.32, vel: new THREE.Vector3(rnd(-0.2, 0.2), 0.7, 0), life: 1.3 });
    }
    if (sit && Math.random() < dt * 1.6 && c.status !== 'coding' &&
        (c.status === 'working' || c.status === 'reading' || c.status === 'reviewing' || c.status === 'completed')) {
        const fx = ROLE_FX[c.id];
        const d = desks.get(c.slotIndex);
        if (fx && d)
            spawn({ map: glyphTexture(fx[Math.floor(Math.random() * fx.length)], '#fde68a'), pos: new THREE.Vector3(d.group.position.x + rnd(0.4, 1.1), 1.2, d.group.position.z + 0.3), size: 0.34, vel: new THREE.Vector3(rnd(-0.15, 0.15), 0.65, 0), life: 1.5 });
    }
    if (c.status === 'thinking' && sit && Math.random() < dt * 1.2)
        spawn({ map: glyphTexture('?', '#c4b5fd'), pos: new THREE.Vector3(c.x * U + rnd(-0.2, 0.2), 2.2, c.y * U + SEAT_DZ * sb), size: 0.3, vel: new THREE.Vector3(rnd(-0.2, 0.2), 0.6, 0), life: 1.4 });
}

// #endregion

// #region Etiquetas y pantallas de los escritorios

// Rectángulo redondeado en un canvas 2D.
function roundRect(g, x, y, w, h, r) { g.beginPath(); g.roundRect(x, y, w, h, r); }

// Redibuja la etiqueta flotante (nombre, estado, tarea y globo de diálogo) solo si cambió.
function updateLabel(c) {
    const task = c.task && c.status !== 'idle' ? (c.task.length > 30 ? c.task.slice(0, 29) + '…' : c.task) : '';
    const say = c.say && c.say.until > now ? c.say.text : '';
    const key = [c.name, c.icon, c.status, task, say].join('|');
    if (key === c.labelKey) return;
    c.labelKey = key;

    const { ctx: g, canvas, tex } = c.view.label;
    g.clearRect(0, 0, canvas.width, canvas.height);
    g.textAlign = 'center'; g.textBaseline = 'middle';
    const col = COLORS[c.status];

    g.font = '600 30px Segoe UI, Segoe UI Emoji, sans-serif';
    const label = c.icon + ' ' + c.name;
    const w = g.measureText(label).width + 64, bx = 256 - w / 2;
    g.fillStyle = 'rgba(8,12,30,.9)'; g.strokeStyle = col; g.lineWidth = 3;
    roundRect(g, bx, 190, w, 52, 26); g.fill(); g.stroke();
    g.fillStyle = col; g.beginPath(); g.arc(bx + 24, 216, 8, 0, 7); g.fill();
    g.fillStyle = '#e6e9f8'; g.fillText(label, 256 + 12, 218);
    const b = BUBBLES[c.status];
    if (b) { g.font = '34px Segoe UI Emoji, sans-serif'; g.fillText(b, bx + w + 26, 214); }

    if (task) {
        g.font = '500 24px Segoe UI, sans-serif';
        const tw = g.measureText(task).width + 36;
        g.fillStyle = 'rgba(8,12,30,.85)'; roundRect(g, 256 - tw / 2, 146, tw, 38, 19); g.fill();
        g.fillStyle = '#c7cdee'; g.fillText(task, 256, 166);
    }

    if (say) {
        g.font = '500 26px Segoe UI, Segoe UI Emoji, sans-serif';
        const sw = Math.min(480, g.measureText(say).width + 44), sy = task ? 78 : 112;
        g.fillStyle = '#f8fafc'; roundRect(g, 256 - sw / 2, sy - 18, sw, 52, 24); g.fill();
        g.beginPath(); g.moveTo(244, sy + 33); g.lineTo(256, sy + 50); g.lineTo(268, sy + 33); g.fill();
        g.fillStyle = '#1e1b4b'; g.fillText(say, 256, sy + 9);
    }
    tex.needsUpdate = true;
    c.view.sprite.position.y = 2.05 + (c.sitting ? -0.3 : 0);
}

// Dibuja la pantalla del escritorio según el estado del agente.
function drawScreen(d, status, t) {
    const { ctx: g, canvas, tex } = d.screen;
    const w = canvas.width, h = canvas.height, col = COLORS[status];
    g.fillStyle = '#070b1d'; g.fillRect(0, 0, w, h);
    g.fillStyle = status === 'idle' ? '#0d1330' : col + '30'; g.fillRect(6, 6, w - 12, h - 12);
    g.fillStyle = col;
    if (status === 'coding' || status === 'working') {
        for (let i = 0; i < 7; i++) {
            const len = 30 + 100 * Math.abs(Math.sin(t * 1.3 + i * 1.7));
            g.globalAlpha = 0.5 + 0.5 * Math.abs(Math.sin(t * 2 + i));
            g.fillRect(18 + (i % 3) * 8, 16 + i * 13, len, 6);
        }
        g.globalAlpha = 1; g.fillRect(150, 100, 6, 10 * (Math.floor(t * 2) % 2));
    } else if (status === 'thinking') {
        for (let i = 0; i < 3; i++) { g.globalAlpha = 0.3 + 0.7 * Math.abs(Math.sin(t * 3 + i * 0.8)); g.beginPath(); g.arc(66 + i * 30, 58, 9, 0, 7); g.fill(); }
        g.globalAlpha = 1;
    } else if (status === 'reading' || status === 'reviewing') {
        g.fillStyle = '#e2e8f0'; g.fillRect(24, 14, w - 48, h - 28);
        g.fillStyle = '#94a3b8'; for (let i = 0; i < 6; i++) g.fillRect(34, 24 + i * 14, 90 + (i % 3) * 20, 5);
        g.fillStyle = col; g.fillRect(30, 20 + ((t * 28) % 80), w - 60, 8);
    } else if (status === 'waiting') {
        g.strokeStyle = col; g.lineWidth = 6; g.beginPath(); g.arc(w / 2, h / 2, 32, 0, 7); g.stroke();
        g.lineCap = 'round'; g.beginPath(); g.moveTo(w / 2, h / 2); g.lineTo(w / 2 + Math.sin(t * 2) * 24, h / 2 - Math.cos(t * 2) * 24); g.stroke();
    } else if (status === 'completed') {
        g.strokeStyle = col; g.lineWidth = 12; g.lineCap = 'round'; g.lineJoin = 'round';
        g.beginPath(); g.moveTo(66, 62); g.lineTo(88, 84); g.lineTo(130, 36); g.stroke();
    } else if (status === 'error') {
        g.globalAlpha = 0.4 + 0.6 * Math.abs(Math.sin(t * 6)); g.fillRect(6, 6, w - 12, h - 12); g.globalAlpha = 1;
        g.fillStyle = '#fff'; g.font = '700 50px sans-serif'; g.textAlign = 'center'; g.fillText('!', w / 2, h / 2 + 18);
    } else {
        g.globalAlpha = 0.45; g.fillRect(20, 30, 90, 6); g.fillRect(20, 46, 60, 6); g.globalAlpha = 1;
        if (Math.floor(t * 1.5) % 2) g.fillRect(20, 66, 6, 12);
    }
    tex.needsUpdate = true;
}

// #endregion

// #region Actualización de la escena en cada cuadro

// Actualiza luces, pantallas y sillas de los escritorios; oculta los que no tienen dueño en la oficina activa.
function updateDesks(dt) {
    const owners = new Map();
    chars.forEach(c => owners.set(c.slotIndex, c));
    desks.forEach((d, i) => {
        const c = owners.get(i);
        d.group.visible = !!c;                       // only the active office's desks are shown
        const st = c ? c.status : 'idle';
        const col = new THREE.Color(COLORS[st]);
        const active = st !== 'idle';
        const pulse = 0.65 + 0.35 * Math.sin(now * 4 + i);
        d.light.color.copy(col); d.light.intensity = active ? 9 * pulse : 0;
        d.decal.material.color.copy(col); d.decal.material.opacity = lerp(d.decal.material.opacity, active ? 0.28 * pulse : 0, Math.min(1, dt * 6));
        const accent = c ? new THREE.Color(c.color) : col;
        d.led.color.copy(accent); d.led.emissive.copy(accent);
        d.bezelGlow.material.color.copy(col); d.bezelGlow.material.emissive.copy(col);
        const typing = c && c.sitting && (st === 'coding' || st === 'working');
        d.keyMat.emissive.setHex(typing ? 0x1d4ed8 : 0x000000); d.keyMat.emissiveIntensity = typing ? 0.8 + 0.6 * Math.sin(now * 18) : 0;
        if (now - d.lastDraw > (active ? 0.07 : 0.4)) { d.lastDraw = now; drawScreen(d, st, now); }
    });
    chars.forEach(c => { const d = desks.get(c.slotIndex); if (d) d.chair.position.z = -0.75 + SEAT_DZ * c.sitBlend; });

    const orch = [...chars.values()].find(c => c.isOrch);
    const busy = !!orch && (orch.status === 'waiting' || orch.status === 'reviewing');
    if (busy || whiteboard.busy !== busy) { whiteboard.busy = busy; drawWhiteboard(now); }
    if (tvDirty) { tvDirty = false; drawTv(); }

    if (clockHands) {
        const d = new Date();
        clockHands.s.rotation.z = -d.getSeconds() / 60 * Math.PI * 2;
        clockHands.m.rotation.z = -(d.getMinutes() + d.getSeconds() / 60) / 60 * Math.PI * 2;
        clockHands.h.rotation.z = -((d.getHours() % 12) + d.getMinutes() / 60) / 12 * Math.PI * 2;
    }
}

// Anima la impresora y la cafetera.
function updateProps(dt) {
    // Impresora: sale papel mientras se "imprime" un documento.
    const printing = now < printer.busyUntil;
    printer.paper.visible = printing;
    if (printing) {
        const k = 1 - (printer.busyUntil - now) / 2.4;
        printer.paper.position.z = 0.45 + Math.min(1, k) * 0.35;
        printer.led.color.setHex(0xfbbf24); printer.led.emissive.setHex(0xfbbf24);
        if (Math.random() < dt * 6) spawn({ map: softTexture(), color: 0xfde68a, additive: true, pos: printer.pos.clone().add(new THREE.Vector3(rnd(-0.3, 0.3), 0.1, 0)), size: 0.25, vel: new THREE.Vector3(0, 0.4, 0.2), life: 0.7 });
    } else { printer.led.color.setHex(0x38bdf8); printer.led.emissive.setHex(0x38bdf8); }

    // Cafetera: sale vapor mientras alguien toma café.
    if (now < coffee.busyUntil && Math.random() < dt * 8)
        spawn({ map: softTexture(), color: 0xffffff, pos: coffee.pos.clone().add(new THREE.Vector3(rnd(-0.05, 0.05), 0, rnd(-0.05, 0.05))), size: 0.2, vel: new THREE.Vector3(rnd(-0.1, 0.1), 0.6, 0), life: 1.4, grow: 1.8 });
    coffee.led.color.setHex(now < coffee.busyUntil ? 0xfbbf24 : 0x4ade80); coffee.led.emissive.copy(coffee.led.color);
}

// Anima y retira los aros de pulso de las asignaciones.
function updatePulses() {
    for (let i = pulses.length - 1; i >= 0; i--) {
        const p = pulses[i], k = (now - p.t0) / 1.6;
        if (k >= 1) { scene.remove(p.mesh); p.mesh.geometry.dispose(); p.mesh.material.dispose(); pulses.splice(i, 1); continue; }
        p.mesh.scale.setScalar(1 + k * 2.2); p.mesh.material.opacity = (1 - k) * 0.9;
    }
}

// Crea un aro de pulso bajo el puesto de un agente.
function addPulse(c, colorHex) {
    const mesh = new THREE.Mesh(new THREE.RingGeometry(0.5, 0.62, 40),
        new THREE.MeshBasicMaterial({ color: colorHex, transparent: true, opacity: 0.9, side: THREE.DoubleSide, depthWrite: false }));
    mesh.rotation.x = -Math.PI / 2; mesh.position.set((c.slot.x + 1) * U, 0.05, (c.slot.y + 0.2) * U);
    scene.add(mesh); pulses.push({ mesh, t0: now });
}

// Mueve la cámara con suavidad: rotación, zoom y seguimiento del personaje seleccionado.
function updateCamera(dt) {
    cam.yaw = lerp(cam.yaw, cam.ty, Math.min(1, dt * 8));
    cam.pitch = lerp(cam.pitch, cam.tp, Math.min(1, dt * 8));
    cam.zoom = lerp(cam.zoom, cam.tz, Math.min(1, dt * 8));

    // Sigue con suavidad al personaje seleccionado; si no hay, mira el centro de la sala.
    const sel = selectedId ? chars.get(selectedId) : null;
    const goal = sel ? new THREE.Vector3(sel.x * U, 1.1, sel.y * U) : baseTarget;
    target.lerp(goal, Math.min(1, dt * 4));

    const w = canvas.clientWidth || 800, h = canvas.clientHeight || 450;
    const vfov = THREE.MathUtils.degToRad(camera.fov), hfov = 2 * Math.atan(Math.tan(vfov / 2) * (w / h));
    const dist = (13.4 / Math.sin(Math.min(vfov, hfov) / 2)) * cam.zoom * (sel ? 0.5 : 1);
    const y = cam.yaw + (cam.dragging || sel ? 0 : Math.sin(now * 0.15) * 0.03);
    camera.position.set(
        target.x + dist * Math.cos(cam.pitch) * Math.sin(y),
        target.y + dist * Math.sin(cam.pitch),
        target.z + dist * Math.cos(cam.pitch) * Math.cos(y));
    camera.lookAt(target);
}

// Bucle de animación: avanza el tiempo, actualiza personajes y escena y dibuja.
function frame(ts) {
    raf = requestAnimationFrame(frame);
    const dt = Math.min(0.05, (ts - last) / 1000 || 0);
    last = ts; now = ts / 1000;

    chars.forEach(c => { stepChar(c, dt); poseChar(c, dt); updateLabel(c); });
    updateDesks(dt);
    updateProps(dt);
    updatePulses();
    updateParticles(dt);
    if (holo) { holo.rotation.z += dt * 0.8; holo.position.y = 1.6 + Math.sin(now * 1.5) * 0.06; holo.userData.core.rotation.y += dt; holo.userData.core.position.y = holo.position.y; }
    updateCamera(dt);
    renderer.render(scene, camera);
}

// Ajusta el tamaño del dibujo al contenedor.
function resize() {
    if (!renderer) return;
    const r = canvas.getBoundingClientRect();
    const w = Math.max(200, r.width), h = Math.max(160, r.height);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
    renderer.setSize(w, h, false);
    camera.aspect = w / h; camera.updateProjectionMatrix();
}

// #endregion

// #region Interacción (ratón, selección y cámara)

// Registra un evento y guarda cómo quitarlo al cerrar la escena.
function listen(el, type, fn, opts) { el.addEventListener(type, fn, opts); listeners.push(() => el.removeEventListener(type, fn, opts)); }

// Personaje bajo el cursor, o null.
function pick(e) {
    const r = canvas.getBoundingClientRect();
    pointer.set(((e.clientX - r.left) / r.width) * 2 - 1, -((e.clientY - r.top) / r.height) * 2 + 1);
    raycaster.setFromCamera(pointer, camera);
    const hits = raycaster.intersectObjects([...chars.values()].map(c => c.view.hit), false);
    return hits.length ? hits[0].object.userData.char : null;
}

// Selecciona (o deselecciona) un personaje y avisa a Blazor.
function select(id) {
    selectedId = id;
    cam.tz = id ? 0.9 : 1;
    dotnet?.invokeMethodAsync('OnAgentSelected', id).catch(() => { });
}

// Conecta ratón y rueda: rotar, zoom, seleccionar y restablecer la cámara.
function bindInput() {
    listen(canvas, 'pointerdown', e => {
        cam.dragging = true; cam.lastX = e.clientX; cam.lastY = e.clientY; cam.moved = 0;
        canvas.setPointerCapture(e.pointerId); canvas.style.cursor = 'grabbing'; tip.style.display = 'none';
    });
    listen(canvas, 'pointermove', e => {
        if (cam.dragging) {
            const dx = e.clientX - cam.lastX, dy = e.clientY - cam.lastY;
            cam.lastX = e.clientX; cam.lastY = e.clientY; cam.moved += Math.abs(dx) + Math.abs(dy);
            cam.ty = THREE.MathUtils.clamp(cam.ty - dx * 0.006, 0.12, 1.45);
            cam.tp = THREE.MathUtils.clamp(cam.tp + dy * 0.005, 0.3, 1.1);
            return;
        }
        const c = pick(e);
        canvas.style.cursor = c ? 'pointer' : 'grab';
        if (!c) { tip.style.display = 'none'; return; }
        const r = canvas.getBoundingClientRect();
        tip.innerHTML = `<b>${c.icon} ${c.name}</b><span>${c.role}</span><em style="color:${COLORS[c.status]}">● ${LABELS[c.status]}</em>` +
            (c.task ? `<small>${c.task.replace(/</g, '&lt;')}</small>` : '') + '<small style="opacity:.6">Clic para seguirlo</small>';
        tip.style.display = 'grid';
        tip.style.left = Math.min(r.width - 220, e.clientX - r.left + 14) + 'px';
        tip.style.top = (e.clientY - r.top + 14) + 'px';
    });
    listen(canvas, 'pointerup', e => {
        const wasClick = cam.dragging && cam.moved < 5;
        cam.dragging = false; canvas.style.cursor = 'grab';
        try { canvas.releasePointerCapture(e.pointerId); } catch { }
        if (wasClick) { const c = pick(e); select(c ? (c.id === selectedId ? null : c.id) : null); }
    });
    listen(canvas, 'pointerleave', () => { tip.style.display = 'none'; });
    listen(canvas, 'wheel', e => { e.preventDefault(); cam.tz = THREE.MathUtils.clamp(cam.tz * (1 + Math.sign(e.deltaY) * 0.08), 0.35, 1.35); }, { passive: false });
    listen(canvas, 'dblclick', () => { cam.ty = 0.78; cam.tp = 0.62; cam.tz = 1; if (selectedId) select(null); });
    canvas.style.cursor = 'grab';
}

const agentsOf = () => [...chars.values()];
const short = t => (t && t.length > 34 ? t.slice(0, 33) + '…' : t || 'Nueva tarea');

// #endregion

// #region API pública y coreografías de eventos

// Coreografía de entrega: tras terminar, el agente (con paso por la impresora si hay documento) lleva su
// resultado al siguiente agente o al Orquestador.
function deliver(c, orch, hasDocument) {
    const dependents = agentsOf().filter(o => o !== c && o.waitingFor.includes(c.id));
    const targets = dependents.length ? dependents : (orch ? [orch] : []);

    if (hasDocument) {
        // Los documentos salen de la impresora antes de entregarse.
        c.trips.push({
            tile: PRINTER_SPOT, dwell: 2600, emote: '🖨️ Imprimiendo el documento…',
            onArrive: () => { printer.busyUntil = now + 2.4; }
        });
    }

    targets.forEach(t => {
        c.trips.push({
            tile: () => (t.sitting ? t.approach() : t.tile()), carry: hasDocument ? '📄' : '📄', dwell: 1500,
            emote: dependents.length ? '📄 Te paso mi resultado' : '📄 Tarea entregada', lookAt: () => t,
            onArrive: () => {
                t.waitingFor = t.waitingFor.filter(id => id !== c.id);
                t.speak('📥 Recibido', 2.5); t.look = c;
                t.startMicro('highfive', 1.2); c.startMicro('highfive', 1.2);
                sparkles(new THREE.Vector3(c.x * U, 2.0, c.y * U), '🙌', 3);
            }
        });
    });
}

// Los libros viajan de la estantería al escritorio del investigador (él nunca se levanta).
function flyBooks(c) {
    const from = new THREE.Vector3(1.0 * U, 2.0, 3.5 * U);
    const d = desks.get(c.slotIndex);
    const to = d ? new THREE.Vector3(d.group.position.x + 0.75, 1.35, d.group.position.z + 0.3) : c.worldPos().add(new THREE.Vector3(0, 1.4, 0));
    for (let i = 0; i < 5; i++) {
        setTimeout(() => {
            if (!scene) return;
            const life = 1.6;
            spawn({ map: glyphTexture('📚'), pos: from.clone().add(new THREE.Vector3(rnd(-0.3, 0.3), rnd(0, 0.5), 0)), size: 0.5,
                vel: to.clone().sub(from).divideScalar(life), life });
        }, i * 280);
    }
}

// El Orquestador felicita a TODOS los agentes que participaron, uno por uno, y luego todo el equipo celebra.
function congratulate(orch) {
    const team = [...participants].map(id => chars.get(id)).filter(a => a && !a.isOrch);
    participants.clear();
    if (!orch || !team.length) {
        confetti(COLS * U / 2, ROWS * U / 2 - 1, 80);
        agentsOf().forEach(a => { if (a.sitting) a.startMicro('cheer', 1.8); });
        restUntil = now + 8;
        return;
    }
    team.forEach(a => {
        orch.trips.push({
            tile: () => (a.sitting ? a.approach() : a.tile()), dwell: 2300, lookAt: () => a, micro: 'wave',
            emote: '👏 ¡Bien hecho, ' + a.name + '!',
            onArrive: () => {
                a.look = orch; a.speak('🙌 ¡Gracias!', 2.2); a.startMicro('cheer', 1.8);
                addPulse(a, 0x4ade80);
                sparkles(new THREE.Vector3(a.x * U, 2.2, a.y * U), '👏', 6);
            }
        });
    });
    orch.trips.push({
        tile: celebrateSpot(), dwell: 3500, micro: 'cheer', emote: '🎉 ¡Gran trabajo, equipo!',
        onArrive: () => {
            confetti(COLS * U / 2, ROWS * U / 2 - 1, 120);
            agentsOf().forEach(a => { if (!a.isOrch && a.sitting) { a.startMicro('clap', 2.4); a.speak('🎉', 2.6); } });
        },
        onLeave: () => { restUntil = now + 6; }
    });
    restUntil = now + 40;     // the celebration walk takes a while: keep the social life paused until it ends
}

const api = {
    // Crea el renderizador y construye la escena.
    init(canvasEl, tipEl, dotnetRef) {
        this.dispose();
        canvas = canvasEl; tip = tipEl; dotnet = dotnetRef || null;
        renderer = new THREE.WebGLRenderer({ canvas, antialias: true, alpha: false, powerPreference: 'high-performance' });
        renderer.shadowMap.enabled = true; renderer.shadowMap.type = THREE.PCFSoftShadowMap;
        renderer.toneMapping = THREE.ACESFilmicToneMapping; renderer.toneMappingExposure = 1.15;
        renderer.outputColorSpace = THREE.SRGBColorSpace;
        camera = new THREE.PerspectiveCamera(32, 16 / 9, 0.5, 220);
        buildWorld();
        resize();
        resizeObs = new ResizeObserver(resize); resizeObs.observe(canvas.parentElement || canvas);
        bindInput();
        last = performance.now(); raf = requestAnimationFrame(frame);
    },

    // Sincroniza los personajes con la lista de agentes; si cambió la oficina, rehace los puestos.
    setAgents(list, resync) {
        // Otra lista de agentes (se abrió otra oficina): se rehace la distribución de puestos desde cero.
        const ids = list.map(m => m.id).join('|');
        if (ids !== rosterKey) {
            [...chars.values()].forEach(c => scene.remove(c.view.group));
            chars.clear(); participants.clear(); socialBusy = false; orchActive = false; restUntil = 0;
            rosterKey = ids; resync = true;
        }
        const seen = new Set();
        let free = 1;
        list.forEach(m => {
            seen.add(m.id);
            let c = chars.get(m.id);
            if (!c) {
                const idx = m.isOrchestrator ? 0 : Math.min(free++, SLOTS.length - 1);
                c = new Char(m, SLOTS[idx], idx); chars.set(m.id, c);
                c.setStatus((m.status || 'idle').toLowerCase(), m.task);
            } else {
                c.update(m);
                if (resync) c.setStatus((m.status || 'idle').toLowerCase(), m.task);
            }
        });
        [...chars.keys()].forEach(id => { if (!seen.has(id)) { scene.remove(chars.get(id).view.group); chars.delete(id); } });
        tvDirty = true;
    },

    // Reacciona a un evento real de la oficina: asignación, entrega, error, consulta a la base o fin de la
    // solicitud.
    event(e) {
        const c = chars.get(e.agentId);
        const d = e.additionalData || {};
        if (c && d.agentStatus) c.setStatus(String(d.agentStatus).toLowerCase(), d.taskTitle);
        const orch = agentsOf().find(x => x.isOrch);
        switch (e.eventType) {
            case 'AgentTaskAssigned':
                cancelSocial(); orchActive = true;
                if (c && !c.isOrch) {
                    participants.add(c.id);
                    c.waitingFor = (d.dependsOnAgents || '').split(',').filter(Boolean);
                    addPulse(c, 0xfbbf24);
                    if (orch) orch.trips.push({
                        tile: () => c.approach(), carry: '📌', dwell: 1300,
                        emote: '📌 ' + short(d.taskTitle || e.description), lookAt: () => c,
                        onArrive: () => { c.speak('📥 ¡Entendido!', 2.5); c.look = orch; addPulse(c, 0xfde68a); c.startMicro('highfive', 1); }
                    });
                }
                break;
            case 'AgentCompleted':
                if (c && !c.isOrch) { addPulse(c, 0x4ade80); sparkles(new THREE.Vector3(c.x * U, 2.1, c.y * U), '✨', 6); deliver(c, orch, !!d.document); }
                else if (c && c.isOrch) c.speak('✅ ¡Listo!', 3);
                break;
            case 'OrchestrationCompleted':
                orchActive = false;
                lastTvLine = 'Solicitud completada ✓'; tvDirty = true;
                congratulate(orch);
                break;
            case 'AgentError':
                if (c) { c.speak('⚠️ Algo salió mal', 4); addPulse(c, 0xf87171); }
                break;
            case 'KnowledgeConsulted':
                if (c) {
                    // El investigador se queda en su escritorio: en su lugar vuelan libros desde la estantería.
                    c.speak('📚 ' + (d.code || 'Base de conocimiento'), 4);
                    flyBooks(c);
                    lastTvLine = 'Consultando ' + (d.code || 'la base'); tvDirty = true;
                }
                break;
            case 'OrchestrationStarted':
                cancelSocial(); orchActive = true; participants.clear();
                lastTvLine = 'Nueva solicitud'; tvDirty = true;
                break;
        }
    },

    // Selecciona un personaje desde Blazor.
    select(id) { select(id); },

    // Foto para diagnóstico: quién camina o está sentado ahora (la usan las pruebas y la consola del navegador).
    debug() { return agentsOf().map(c => ({ id: c.id, status: c.status, moving: c.moving, sitting: c.sitting, trips: c.trips.length, trip: !!c.trip, say: c.say && c.say.until > now ? c.say.text : '' })); },

    // Libera el renderizador, los eventos y la memoria de la escena.
    dispose() {
        if (raf) cancelAnimationFrame(raf);
        raf = 0;
        if (resizeObs) resizeObs.disconnect();
        listeners.splice(0).forEach(off => off());
        chars.clear(); desks.clear(); pulses.length = 0; particles.length = 0; texCache.clear();
        socialBusy = false; selectedId = null; dotnet = null; rosterKey = ''; orchActive = false; restUntil = 0; participants.clear();
        if (renderer) { renderer.dispose(); renderer.forceContextLoss?.(); renderer = null; }
        scene = null;
    }
};

window.officeScene = api;
window.__officeSceneResolve?.();

// #endregion
