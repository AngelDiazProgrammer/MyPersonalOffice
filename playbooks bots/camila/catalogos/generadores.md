---
bot: camila
catalog: generators
---
## Generadores de IA

Prompts que el bot original enviaba a un modelo generativo. LectorUML no los ejecuta.

### Generador · actualizar_estado_pdi

Se usa en: 02.1. Conversacional Evaluacion Desempeño › PDI - Actualizar estado (al entrar); 03.1. Conversacional Lider Misión › PDI - Actualizar estado (al entrar)

Prompt:

> PDI — ACTUALIZADOR PRODUCCIÓN v17.1.1.4 — FIREWALL A2_20
> 
> ENTRADAS
> ESTADO=$estado_anterior
> CAMILA=$respuesta_camila
> USUARIO=$mensaje_usuario
> 
> SALIDA
> Devuelve SOLO JSON PURO con EXACTAMENTE estas 13 claves:
> A1_70,A2_70,A3_70,A1_20,A2_20,R1_10,enlaceR1_10,R2_10,enlaceR2_10,
> rechazadas70,rechazadas20,rechazados10,etapa
> 
> ESTADO_VACIO={"A1_70":null,"A2_70":null,"A3_70":null,"A1_20":null,"A2_20":null,"R1_10":null,"enlaceR1_10":null,"R2_10":null,"enlaceR2_10":null,"rechazadas70":[],"rechazadas20":[],"rechazados10":[],"etapa":"A1_70"}
> 
> ====================================================================
> 0. PRINCIPIO ÚNICO
> ====================================================================
> 
> ESTADO es la única fuente de verdad.
> 
> 1. Parsea ESTADO quitando únicamente fences ```json y ```.
> 2. Si no parsea, usa ESTADO_VACIO.
> 3. null en listas => [].
> 4. Define BASE=copia profunda e INMUTABLE de ESTADO.
> 5. Define OUT=copia profunda EXACTA de BASE.
> 
> REGLA ABSOLUTA:
> CADA RAMA empieza desde OUT=BASE y modifica SOLO los campos que esa rama enumera.
> Todo campo no enumerado DEBE quedar idéntico a BASE.
> 
> NUNCA:
> - reconstruyas slots desde CAMILA;
> - compactes o muevas slots;
> - borres un slot aceptado anterior;
> - borres listas históricas completas;
> - cambies más de una familia en una ejecución.
> 
> ====================================================================
> 1. NORMALIZACIÓN
> ====================================================================
> 
> C=CAMILA o "".
> u=USUARIO o "" en minúsculas, trim, espacios simples.
> C_NORM=C sin asteriscos Markdown y con espacios normalizados.
> 
> CANON(x):
> - null => null
> - minúsculas, trim, espacios simples
> - elimina asteriscos Markdown externos
> - elimina SOLO un prefijo funcional inicial si existe:
>   "ACCIÓN 1 DEL 70%:"
>   "ACCIÓN 2 DEL 70%:"
>   "ACCIÓN 3 DEL 70%:"
>   "ACCIÓN 1 DEL 20%:"
>   "ACCIÓN 2 DEL 20%:"
>   "RECURSO 1 DEL 10%:"
>   "RECURSO 2 DEL 10%:"
> - ignora . , : ; únicamente al final
> 
> LIMPIA_TITULO(x):
> - conserva texto/puntuación interna;
> - elimina asteriscos externos;
> - elimina SOLO el prefijo funcional inicial anterior;
> - trim;
> - NO elimina "Curso:" ni "Taller:".
> 
> AGREGA_UNICO(L,T):
> copia L; agrega T al final SOLO si CANON(T) no existe en CANON(L).
> 
> REMOVE_SOLO(L,T):
> copia L conservando orden;
> elimina SOLO x donde CANON(x)==CANON(T);
> conserva TODOS los demás exactamente.
> 
> ====================================================================
> 2. GUARDAS QUE TERMINAN INMEDIATAMENTE
> ====================================================================
> 
> Si trim(C)=="INICIAR PLAN":
> devuelve ESTADO_VACIO. FIN.
> 
> Si BASE.etapa=="FECHAS":
> devuelve BASE. FIN.
> 
> FECHA=true si u:
> - contiene enero/febrero/marzo/abril/mayo/junio/julio/agosto/septiembre/octubre/noviembre/diciembre;
> - o parece dd/mm, dd-mm, dd/mm/yyyy, yyyy-mm-dd;
> - o es "mañana" o "pasado mañana".
> 
> Si FECHA=true:
> devuelve BASE EXACTO. FIN.
> Una fecha JAMÁS confirma el resumen ni una acción.
> 
> NEGATIVO=true SOLO si u (ignorando puntuación final) es:
> "no","no gracias","no me gusta","esa no","ese no","no me convence",
> "otra","otro","otra opcion","otra opción","cambiala","cámbiala","cambialo","cámbialo".
> 
> AFIRMATIVO=true SOLO si NEGATIVO=false y u es:
> "si","sí","acepto","aceptar","dale","de una","perfecto","claro",
> "me gusta","si me gusta","sí me gusta","esa si","esa sí","ese si","ese sí",
> "quiero esa","quiero ese","la acepto","lo acepto".
> 
> SOLO=true SOLO si u contiene:
> "solo con la primera","solo la primera","me quedo con la primera",
> "con uno basta","con una basta","solo ese","solo el 1",
> "no quiero segunda accion","no quiero segunda acción",
> "pasemos al 10","pasemos al 10%","continuemos al 10","continuemos con el 10".
> 
> OMITIR_A3=true SOLO si u contiene:
> "con dos basta","solo dos","solo quiero dos","me quedo con dos",
> "con las dos basta","no quiero tercera","no quiero una tercera",
> "pasemos al 20","pasemos al 20%","continuemos al 20","continuemos con el 20".
> 
> Si no hay AFIRMATIVO ni NEGATIVO ni SOLO ni OMITIR_A3:
> devuelve BASE EXACTO. FIN.
> 
> ====================================================================
> 2.3 FIREWALL A1_70 — RECHAZO/ACEPTACIÓN TEMPRANA
> ====================================================================
> 
> Si BASE.A1_70==null:
> 
>   ACCION70_VISIBLE=true si C_NORM contiene cualquiera:
>   "ACCIÓN 1 DEL 70%:"
>   "ACCIÓN 2 DEL 70%:"
>   "ACCIÓN 3 DEL 70%:"
> 
>   Si ACCION70_VISIBLE=true:
>     TITULO_A1=texto de la misma línea después del marcador 70%, usando LIMPIA_TITULO.
>     OUT=BASE exacto.
> 
>     Si NEGATIVO:
>       OUT.A1_70=null
>       OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO_A1)
>       OUT.etapa="A1_70"
>       devuelve OUT. FIN ABSOLUTO.
> 
>     Si AFIRMATIVO:
>       OUT.A1_70=TITULO_A1
>       OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO_A1)
>       OUT.etapa="A2_70"
>       devuelve OUT. FIN ABSOLUTO.
> 
>     devuelve BASE EXACTO. FIN ABSOLUTO.
> 
> CONTROL:
> BASE.A1_70=null
> CAMILA="ACCIÓN 1 DEL 70%: Crea un tablero..."
> USUARIO="no"
> => A1_70=null
> => rechazadas70=["Crea un tablero..."]
> => etapa="A1_70"
> => jamás puede avanzar a A2_70.
> 
> ====================================================================
> 2.4 FIREWALL A2_70 — NO PERMITIR SALTO A 20%/10%
> ====================================================================
> 
> Si BASE.etapa=="A2_70"
> Y BASE.A1_70!=null
> Y BASE.A2_70==null:
> 
>   ACCION70_VISIBLE=true si C_NORM contiene cualquiera:
>   "ACCIÓN 1 DEL 70%:"
>   "ACCIÓN 2 DEL 70%:"
>   "ACCIÓN 3 DEL 70%:"
> 
>   ACCION20_O_10_VISIBLE=true si C_NORM contiene cualquiera:
>   "ACCIÓN 1 DEL 20%:"
>   "ACCIÓN 2 DEL 20%:"
>   "RECURSO 1 DEL 10%:"
>   "RECURSO 2 DEL 10%:"
> 
>   Si ACCION20_O_10_VISIBLE=true:
>     devuelve BASE EXACTO. FIN ABSOLUTO.
> 
>   Si ACCION70_VISIBLE=true:
>     TITULO_A2=texto de la misma línea después del marcador visible 70%, usando LIMPIA_TITULO.
>     OUT=BASE exacto.
> 
>     Si NEGATIVO:
>       OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO_A2)
>       OUT.etapa="A2_70"
>       devuelve OUT. FIN ABSOLUTO.
> 
>     Si AFIRMATIVO:
>       si CANON(TITULO_A2)==CANON(BASE.A1_70)
>       o CANON(TITULO_A2) existe en CANON(BASE.rechazadas70):
>         devuelve BASE EXACTO. FIN ABSOLUTO.
> 
>       OUT.A2_70=TITULO_A2
>       OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO_A2)
>       OUT.etapa="A3_70"
>       devuelve OUT. FIN ABSOLUTO.
> 
>   devuelve BASE EXACTO. FIN ABSOLUTO.
> 
> CONTROL CRÍTICO:
> BASE:
> A1_70="Motiva..."
> A2_70=null
> A1_20=null
> A2_20=null
> etapa="A2_70"
> 
> CAMILA="ACCIÓN 2 DEL 20%: Pide a su jefe..."
> USUARIO="si"
> 
> => devuelve BASE EXACTO.
> => A2_20 DEBE seguir null.
> => A1_20 DEBE seguir null.
> => etapa DEBE seguir "A2_70".
> 
> ====================================================================
> 2.45 FIREWALL A1_20 — OBLIGATORIO
> ====================================================================
> 
> Si BASE.A1_70!=null
> Y BASE.A2_70!=null
> Y BASE.A1_20==null:
> 
>   ACCION20_VISIBLE=true si C_NORM contiene:
>   "ACCIÓN 1 DEL 20%:" o "ACCIÓN 2 DEL 20%:".
> 
>   Si ACCION20_VISIBLE=true:
>     TITULO_A1_20=texto de la misma línea después del marcador visible 20%, usando LIMPIA_TITULO.
>     OUT=BASE exacto.
> 
>     Si NEGATIVO:
>       OUT.A1_20=null
>       OUT.A2_20=BASE.A2_20
>       OUT.rechazadas20=AGREGA_UNICO(BASE.rechazadas20,TITULO_A1_20)
>       OUT.etapa="A1_20"
>       devuelve OUT. FIN ABSOLUTO.
> 
>     Si AFIRMATIVO o SOLO:
>       OUT.A1_20=TITULO_A1_20
>       OUT.A2_20=BASE.A2_20
>       OUT.rechazadas20=REMOVE_SOLO(BASE.rechazadas20,TITULO_A1_20)
>       si SOLO: OUT.etapa="R1_10"
>       si no: OUT.etapa="A2_20"
>       devuelve OUT. FIN ABSOLUTO.
> 
>     devuelve BASE EXACTO. FIN ABSOLUTO.
> 
> CONTROL CRÍTICO:
> BASE.A1_20=null
> BASE.rechazadas20=["Pide feedback..."]
> BASE.etapa="A1_20"
> CAMILA="ACCIÓN 2 DEL 20%: Solicita lluvia de ideas..."
> USUARIO="si"
> => OUT.A1_20="Solicita lluvia de ideas..."
> => OUT.A2_20 sigue null
> => rechazadas20 conserva "Pide feedback..."
> => etapa="A2_20".
> 
> ====================================================================
> 2.46 FIREWALL A2_20 — RECHAZO/ACEPTACIÓN TEMPRANA
> ====================================================================
> 
> Si BASE.etapa=="A2_20"
> Y BASE.A1_20!=null
> Y BASE.A2_20==null
> Y C_NORM contiene "ACCIÓN 2 DEL 20%:":
> 
>   TITULO_A2_20=texto de la misma línea después de "ACCIÓN 2 DEL 20%:", usando LIMPIA_TITULO.
>   OUT=BASE exacto.
> 
>   Si NEGATIVO:
>     OUT.rechazadas20=AGREGA_UNICO(BASE.rechazadas20,TITULO_A2_20)
>     OUT.etapa="A2_20"
>     devuelve OUT. FIN ABSOLUTO.
> 
>   Si SOLO:
>     OUT.etapa="R1_10"
>     devuelve OUT. FIN ABSOLUTO.
> 
>   Si AFIRMATIVO:
>     si CANON(TITULO_A2_20)==CANON(BASE.A1_20)
>     o CANON(TITULO_A2_20) existe en CANON(BASE.rechazadas20):
>       devuelve BASE EXACTO. FIN ABSOLUTO.
>     OUT.A2_20=TITULO_A2_20
>     OUT.rechazadas20=REMOVE_SOLO(BASE.rechazadas20,TITULO_A2_20)
>     OUT.etapa="R1_10"
>     devuelve OUT. FIN ABSOLUTO.
> 
>   devuelve BASE EXACTO. FIN ABSOLUTO.
> 
> CONTROL:
> A1_20="Asesorar..."
> A2_20=null
> rechazadas20=[]
> CAMILA="ACCIÓN 2 DEL 20%: Coordinar..."
> USUARIO="no"
> => A2_20=null
> => rechazadas20=["Coordinar..."]
> => etapa=A2_20
> => Coordinar NO puede volver a presentarse.
> 
> ====================================================================
> 2.5 FIREWALL R1_10 — PRESERVACIÓN DE RECHAZADOS
> ====================================================================
> 
> R1_VISIBLE=true si C_NORM contiene:
> "RECURSO 1 DEL 10%:"
> y "¿Confirmas que quieres aceptar el RECURSO 1 DEL 10%?"
> 
> Si R1_VISIBLE=true y BASE.etapa in ["R1_10","A2_20"] y BASE.R1_10==null:
> 
>  TITULO_R1=texto de la misma línea después de "RECURSO 1 DEL 10%:", usando LIMPIA_TITULO.
>  URL_R1=primera URL http/https asociada al recurso.
> 
>  Si NEGATIVO:
>    OUT=BASE exacto.
>    OUT.rechazados10=AGREGA_UNICO(BASE.rechazados10,TITULO_R1).
>    OUT.etapa="R1_10".
>    devuelve OUT. FIN ABSOLUTO.
> 
>  Si AFIRMATIVO o SOLO:
>    OUT=BASE exacto.
>    OUT.R1_10=TITULO_R1.
>    OUT.enlaceR1_10=URL_R1.
> 
>    # REGLA CRÍTICA:
>    # aceptar un recurso distinto NO borra rechazados anteriores.
>    OUT.rechazados10=copia EXACTA BASE.rechazados10.
>    Si CANON(TITULO_R1) existe en CANON(BASE.rechazados10):
>       elimina SOLO TITULO_R1 de OUT.rechazados10.
> 
>    OUT.etapa="R2_10".
>    devuelve OUT. FIN ABSOLUTO.
> 
> CONTROL OBLIGATORIO:
> BASE.rechazados10=["Curso: Gestión de procesos y mejora continua – EdX."]
> TITULO_R1="Taller: Productividad para líderes de equipos – Udemy."
> USUARIO="si"
> => R1_10=Udemy
> => rechazados10=["Curso: Gestión de procesos y mejora continua – EdX."]
> => EdX NO puede quedar libre otra vez.
> 
> ====================================================================
> 3. RESUMEN — PRIORIDAD MÁXIMA Y CERRADA
> ====================================================================
> 
> RESUMEN=true si C_NORM contiene exactamente la pregunta:
> "¿Te parece alcanzable este plan?"
> 
> SI RESUMEN=true:
> - OUT=BASE exacto.
> - NO evalúes fechas, acciones, recursos ni otras ramas.
> 
> CONFIRMACION_RESUMEN=true SOLO si u, después de trim y quitar .,! finales, es EXACTAMENTE uno de:
> "si","sí","acepto","dale","de una","perfecto","claro","me gusta",
> "si me gusta","sí me gusta","lo acepto","la acepto".
> 
> Si CONFIRMACION_RESUMEN=true:
>  OUT.etapa="FECHAS".
>  devuelve OUT. FIN ABSOLUTO.
> 
> Para CUALQUIER otro USUARIO:
>  OUT.etapa="RESUMEN_FINAL".
>  devuelve OUT. FIN ABSOLUTO.
> 
> CONTROLES:
> USUARIO="el 15 de septiembre" => RESUMEN_FINAL.
> USUARIO="15/09/2026" => RESUMEN_FINAL.
> USUARIO="no" => RESUMEN_FINAL.
> USUARIO="averiguar cosas nuevas" => RESUMEN_FINAL.
> USUARIO="si" => FECHAS.
> USUARIO="sí" => FECHAS.
> 
> NINGÚN slot, enlace o lista puede cambiar en esta rama.
> 
> ====================================================================
> 4. EXTRAER PROPUESTA
> ====================================================================
> 
> Detecta en C_NORM el marcador visible:
> M1="ACCIÓN 1 DEL 70%:"
> M2="ACCIÓN 2 DEL 70%:"
> M3="ACCIÓN 3 DEL 70%:"
> M4="ACCIÓN 1 DEL 20%:"
> M5="ACCIÓN 2 DEL 20%:"
> M6="RECURSO 1 DEL 10%:"
> M7="RECURSO 2 DEL 10%:"
> 
> TITULO = texto de la misma línea después del marcador detectado, usando LIMPIA_TITULO.
> 
> Para M6/M7:
> URL = primera URL http/https asociada a ese recurso antes de la pregunta.
> Si está como [texto](URL), usa URL real.
> 
> Si no hay marcador de confirmación reconocido:
> devuelve BASE. FIN.
> 
> ====================================================================
> 5. SWITCH DETERMINISTA
> ====================================================================
> 
> ------------------------------------------------------------
> 5.1 A1_70
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa=="A1_70"
> Y BASE.A1_70==null
> Y C contiene confirmación de CUALQUIER acción del 70%.
> El rótulo visible NO puede desplazar el slot.
> 
> OUT=BASE exacto.
> 
> Si NEGATIVO:
>  OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO)
>  OUT.etapa="A1_70"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO:
>  OUT.A1_70=TITULO
>  OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO)
>  OUT.etapa="A2_70"
>  devuelve OUT. FIN.
> 
> ------------------------------------------------------------
> 5.2 A2_70
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa=="A2_70"
> Y BASE.A1_70!=null
> Y BASE.A2_70==null
> Y C contiene confirmación de acción 70%.
> 
> OUT=BASE exacto.
> 
> Si NEGATIVO:
>  OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO)
>  OUT.etapa="A2_70"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO:
>  si CANON(TITULO)==CANON(BASE.A1_70) o CANON(TITULO) existe en CANON(BASE.rechazadas70):
>    devuelve BASE. FIN.
>  OUT.A2_70=TITULO
>  OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO)
>  OUT.etapa="A3_70"
>  devuelve OUT. FIN.
> 
> ------------------------------------------------------------
> 5.3 A3_70
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa=="A3_70"
> Y BASE.A1_70!=null
> Y BASE.A2_70!=null
> Y BASE.A3_70==null
> Y C contiene confirmación de acción 70%.
> 
> OUT=BASE exacto.
> 
> Si OMITIR_A3:
>  OUT.etapa="A1_20"
>  devuelve OUT. FIN.
> 
> Si NEGATIVO:
>  OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO)
>  OUT.etapa="A3_70"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO:
>  si CANON(TITULO) está en CANON([BASE.A1_70,BASE.A2_70]+BASE.rechazadas70):
>    OUT.etapa="A1_20"
>    devuelve OUT. FIN.
>  OUT.A3_70=TITULO
>  OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO)
>  OUT.etapa="A1_20"
>  devuelve OUT. FIN.
> 
> ------------------------------------------------------------
> 5.4 A1_20
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa en ["A1_20","A3_70"]
> Y BASE.A1_70!=null
> Y BASE.A2_70!=null
> Y BASE.A1_20==null
> Y C contiene confirmación de "ACCIÓN 1 DEL 20%".
> 
> OUT=BASE exacto.
> 
> Si NEGATIVO:
>  OUT.rechazadas20=AGREGA_UNICO(BASE.rechazadas20,TITULO)
>  OUT.etapa="A1_20"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO o SOLO:
>  OUT.A1_20=TITULO
>  OUT.rechazadas20=REMOVE_SOLO(BASE.rechazadas20,TITULO)
>  si SOLO: OUT.etapa="R1_10"
>  si no: OUT.etapa="A2_20"
>  devuelve OUT. FIN.
> 
> ------------------------------------------------------------
> 5.5 A2_20
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa=="A2_20"
> Y BASE.A1_20!=null
> Y BASE.A2_20==null
> Y C contiene confirmación de "ACCIÓN 2 DEL 20%".
> 
> OUT=BASE exacto.
> IMPORTANTE: OUT.A1_20 permanece EXACTAMENTE BASE.A1_20 en TODA esta rama.
> 
> Si NEGATIVO:
>  OUT.rechazadas20=AGREGA_UNICO(BASE.rechazadas20,TITULO)
>  OUT.etapa="A2_20"
>  devuelve OUT. FIN.
> 
> Si SOLO:
>  OUT.etapa="R1_10"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO:
>  si CANON(TITULO)==CANON(BASE.A1_20) o CANON(TITULO) existe en CANON(BASE.rechazadas20):
>    devuelve BASE. FIN.
>  OUT.A2_20=TITULO
>  OUT.rechazadas20=REMOVE_SOLO(BASE.rechazadas20,TITULO)
>  OUT.etapa="R1_10"
>  devuelve OUT. FIN.
> 
> ------------------------------------------------------------
> 5.6 R1_10
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa en ["R1_10","A2_20"]
> Y BASE.A1_20!=null
> Y BASE.R1_10==null
> Y C contiene confirmación de "RECURSO 1 DEL 10%".
> 
> OUT=BASE exacto.
> IMPORTANTE:
> OUT.A1_20=BASE.A1_20
> OUT.A2_20=BASE.A2_20
> OUT.rechazadas20=BASE.rechazadas20
> 
> Si NEGATIVO:
>  OUT.rechazados10=AGREGA_UNICO(BASE.rechazados10,TITULO)
>  OUT.etapa="R1_10"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO o SOLO:
>  OUT.R1_10=TITULO
>  OUT.enlaceR1_10=URL
>  si CANON(TITULO) existe en CANON(BASE.rechazados10):
>    OUT.rechazados10=REMOVE_SOLO(BASE.rechazados10,TITULO)
>  si CANON(TITULO) NO existe en CANON(BASE.rechazados10):
>    OUT.rechazados10=copia EXACTA BASE.rechazados10
>  OUT.etapa="R2_10"
>  devuelve OUT. FIN.
> 
> CONTROL OBLIGATORIO:
> BASE.rechazados10=[EdX], TITULO=Udemy, AFIRMATIVO
> => OUT.rechazados10=[EdX].
> Udemy NO puede borrar EdX.
> 
> ------------------------------------------------------------
> 5.7 R2_10
> ------------------------------------------------------------
> 
> APLICA si:
> BASE.etapa=="R2_10"
> Y BASE.R1_10!=null
> Y BASE.R2_10==null
> Y C contiene confirmación de "RECURSO 2 DEL 10%".
> 
> OUT=BASE exacto.
> IMPORTANTE: todos los slots 70/20 y R1 permanecen BASE.
> 
> Si NEGATIVO o SOLO:
>  OUT.rechazados10=AGREGA_UNICO(BASE.rechazados10,TITULO) SOLO si NEGATIVO
>  OUT.etapa="RESUMEN_FINAL"
>  devuelve OUT. FIN.
> 
> Si AFIRMATIVO:
>  si CANON(TITULO)==CANON(BASE.R1_10):
>    OUT.etapa="RESUMEN_FINAL"
>    devuelve OUT. FIN.
>  si CANON(TITULO) existe en CANON(BASE.rechazados10):
>    # Un recurso rechazado como R1 jamás puede revivir como R2.
>    OUT.R2_10=null
>    OUT.enlaceR2_10=null
>    OUT.rechazados10=copia EXACTA BASE.rechazados10
>    OUT.etapa="RESUMEN_FINAL"
>    devuelve OUT. FIN ABSOLUTO.
>  OUT.R2_10=TITULO
>  OUT.enlaceR2_10=URL
>  OUT.rechazados10=REMOVE_SOLO(BASE.rechazados10,TITULO)
>  OUT.etapa="RESUMEN_FINAL"
>  devuelve OUT. FIN.
> 
> ====================================================================
> 6. SALIDA SIN RAMA
> ====================================================================
> 
> Devuelve BASE exacto.
> FIN.
> 
> ====================================================================
> 7. INVARIANTES FINALES OBLIGATORIAS
> ====================================================================
> 
> Antes de devolver cualquier OUT verifica:
> 
> 1. Si BASE.A1_20!=null y la rama NO es 5.4:
>    OUT.A1_20 DEBE ser exactamente BASE.A1_20.
> 
> 2. Si BASE.A1_70!=null y la rama no es 5.1:
>    OUT.A1_70=BASE.A1_70.
> 
> 3. Si BASE.A2_70!=null y la rama no es 5.2:
>    OUT.A2_70=BASE.A2_70.
> 
> 4. Si BASE.A3_70!=null y la rama no es 5.3:
>    OUT.A3_70=BASE.A3_70.
> 
> 5. Toda rama 10%:
>    OUT.A1_20=BASE.A1_20
>    OUT.A2_20=BASE.A2_20
>    OUT.rechazadas20=BASE.rechazadas20.
> 
> 6. Para CADA x en BASE.rechazados10:
>    si CANON(x)!=CANON(TITULO aceptado):
>       x DEBE seguir en OUT.rechazados10.
>    Si TITULO aceptado NO estaba en BASE.rechazados10:
>       OUT.rechazados10 DEBE ser IDÉNTICO a BASE.rechazados10.
> 
> 7. Nunca puede existir:
>    A1_20==null y A2_20!=null.
>    Si BASE ya cumple la jerarquía, ninguna rama puede romperla.
> 
> 8. Exactamente 13 claves. Sin Markdown. Sin explicaciones.
> 
> CASO DE REGRESIÓN CRÍTICO:
> BASE:
> A1_20="Sesión quincenal..."
> A2_20=null
> rechazadas20=[]
> etapa="A2_20"
> 
> CAMILA="ACCIÓN 2 DEL 20%: Asesorar..."
> USUARIO="no"
> 
> => A1_20 sigue "Sesión quincenal..."
> => A2_20=null
> => rechazadas20=["Asesorar..."]
> => etapa="A2_20"
> 
> Siguiente:
> CAMILA="ACCIÓN 2 DEL 20%: Coordinar..."
> USUARIO="si"
> 
> => A1_20 sigue "Sesión quincenal..."
> => A2_20="Coordinar..."
> => etapa="R1_10"
> 
> FIN DEL PROMPT.
> 

```json camila:generator
{"name":"actualizar_estado_pdi","id":"7aff5fe0-8cf6-49d8-b4f3-ace553abdd00","prompt":"PDI — ACTUALIZADOR PRODUCCIÓN v17.1.1.4 — FIREWALL A2_20\n\nENTRADAS\nESTADO=$estado_anterior\nCAMILA=$respuesta_camila\nUSUARIO=$mensaje_usuario\n\nSALIDA\nDevuelve SOLO JSON PURO con EXACTAMENTE estas 13 claves:\nA1_70,A2_70,A3_70,A1_20,A2_20,R1_10,enlaceR1_10,R2_10,enlaceR2_10,\nrechazadas70,rechazadas20,rechazados10,etapa\n\nESTADO_VACIO={\"A1_70\":null,\"A2_70\":null,\"A3_70\":null,\"A1_20\":null,\"A2_20\":null,\"R1_10\":null,\"enlaceR1_10\":null,\"R2_10\":null,\"enlaceR2_10\":null,\"rechazadas70\":[],\"rechazadas20\":[],\"rechazados10\":[],\"etapa\":\"A1_70\"}\n\n====================================================================\n0. PRINCIPIO ÚNICO\n====================================================================\n\nESTADO es la única fuente de verdad.\n\n1. Parsea ESTADO quitando únicamente fences \u0060\u0060\u0060json y \u0060\u0060\u0060.\n2. Si no parsea, usa ESTADO_VACIO.\n3. null en listas => [].\n4. Define BASE=copia profunda e INMUTABLE de ESTADO.\n5. Define OUT=copia profunda EXACTA de BASE.\n\nREGLA ABSOLUTA:\nCADA RAMA empieza desde OUT=BASE y modifica SOLO los campos que esa rama enumera.\nTodo campo no enumerado DEBE quedar idéntico a BASE.\n\nNUNCA:\n- reconstruyas slots desde CAMILA;\n- compactes o muevas slots;\n- borres un slot aceptado anterior;\n- borres listas históricas completas;\n- cambies más de una familia en una ejecución.\n\n====================================================================\n1. NORMALIZACIÓN\n====================================================================\n\nC=CAMILA o \"\".\nu=USUARIO o \"\" en minúsculas, trim, espacios simples.\nC_NORM=C sin asteriscos Markdown y con espacios normalizados.\n\nCANON(x):\n- null => null\n- minúsculas, trim, espacios simples\n- elimina asteriscos Markdown externos\n- elimina SOLO un prefijo funcional inicial si existe:\n  \"ACCIÓN 1 DEL 70%:\"\n  \"ACCIÓN 2 DEL 70%:\"\n  \"ACCIÓN 3 DEL 70%:\"\n  \"ACCIÓN 1 DEL 20%:\"\n  \"ACCIÓN 2 DEL 20%:\"\n  \"RECURSO 1 DEL 10%:\"\n  \"RECURSO 2 DEL 10%:\"\n- ignora . , : ; únicamente al final\n\nLIMPIA_TITULO(x):\n- conserva texto/puntuación interna;\n- elimina asteriscos externos;\n- elimina SOLO el prefijo funcional inicial anterior;\n- trim;\n- NO elimina \"Curso:\" ni \"Taller:\".\n\nAGREGA_UNICO(L,T):\ncopia L; agrega T al final SOLO si CANON(T) no existe en CANON(L).\n\nREMOVE_SOLO(L,T):\ncopia L conservando orden;\nelimina SOLO x donde CANON(x)==CANON(T);\nconserva TODOS los demás exactamente.\n\n====================================================================\n2. GUARDAS QUE TERMINAN INMEDIATAMENTE\n====================================================================\n\nSi trim(C)==\"INICIAR PLAN\":\ndevuelve ESTADO_VACIO. FIN.\n\nSi BASE.etapa==\"FECHAS\":\ndevuelve BASE. FIN.\n\nFECHA=true si u:\n- contiene enero/febrero/marzo/abril/mayo/junio/julio/agosto/septiembre/octubre/noviembre/diciembre;\n- o parece dd/mm, dd-mm, dd/mm/yyyy, yyyy-mm-dd;\n- o es \"mañana\" o \"pasado mañana\".\n\nSi FECHA=true:\ndevuelve BASE EXACTO. FIN.\nUna fecha JAMÁS confirma el resumen ni una acción.\n\nNEGATIVO=true SOLO si u (ignorando puntuación final) es:\n\"no\",\"no gracias\",\"no me gusta\",\"esa no\",\"ese no\",\"no me convence\",\n\"otra\",\"otro\",\"otra opcion\",\"otra opción\",\"cambiala\",\"cámbiala\",\"cambialo\",\"cámbialo\".\n\nAFIRMATIVO=true SOLO si NEGATIVO=false y u es:\n\"si\",\"sí\",\"acepto\",\"aceptar\",\"dale\",\"de una\",\"perfecto\",\"claro\",\n\"me gusta\",\"si me gusta\",\"sí me gusta\",\"esa si\",\"esa sí\",\"ese si\",\"ese sí\",\n\"quiero esa\",\"quiero ese\",\"la acepto\",\"lo acepto\".\n\nSOLO=true SOLO si u contiene:\n\"solo con la primera\",\"solo la primera\",\"me quedo con la primera\",\n\"con uno basta\",\"con una basta\",\"solo ese\",\"solo el 1\",\n\"no quiero segunda accion\",\"no quiero segunda acción\",\n\"pasemos al 10\",\"pasemos al 10%\",\"continuemos al 10\",\"continuemos con el 10\".\n\nOMITIR_A3=true SOLO si u contiene:\n\"con dos basta\",\"solo dos\",\"solo quiero dos\",\"me quedo con dos\",\n\"con las dos basta\",\"no quiero tercera\",\"no quiero una tercera\",\n\"pasemos al 20\",\"pasemos al 20%\",\"continuemos al 20\",\"continuemos con el 20\".\n\nSi no hay AFIRMATIVO ni NEGATIVO ni SOLO ni OMITIR_A3:\ndevuelve BASE EXACTO. FIN.\n\n====================================================================\n2.3 FIREWALL A1_70 — RECHAZO/ACEPTACIÓN TEMPRANA\n====================================================================\n\nSi BASE.A1_70==null:\n\n  ACCION70_VISIBLE=true si C_NORM contiene cualquiera:\n  \"ACCIÓN 1 DEL 70%:\"\n  \"ACCIÓN 2 DEL 70%:\"\n  \"ACCIÓN 3 DEL 70%:\"\n\n  Si ACCION70_VISIBLE=true:\n    TITULO_A1=texto de la misma línea después del marcador 70%, usando LIMPIA_TITULO.\n    OUT=BASE exacto.\n\n    Si NEGATIVO:\n      OUT.A1_70=null\n      OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO_A1)\n      OUT.etapa=\"A1_70\"\n      devuelve OUT. FIN ABSOLUTO.\n\n    Si AFIRMATIVO:\n      OUT.A1_70=TITULO_A1\n      OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO_A1)\n      OUT.etapa=\"A2_70\"\n      devuelve OUT. FIN ABSOLUTO.\n\n    devuelve BASE EXACTO. FIN ABSOLUTO.\n\nCONTROL:\nBASE.A1_70=null\nCAMILA=\"ACCIÓN 1 DEL 70%: Crea un tablero...\"\nUSUARIO=\"no\"\n=> A1_70=null\n=> rechazadas70=[\"Crea un tablero...\"]\n=> etapa=\"A1_70\"\n=> jamás puede avanzar a A2_70.\n\n====================================================================\n2.4 FIREWALL A2_70 — NO PERMITIR SALTO A 20%/10%\n====================================================================\n\nSi BASE.etapa==\"A2_70\"\nY BASE.A1_70!=null\nY BASE.A2_70==null:\n\n  ACCION70_VISIBLE=true si C_NORM contiene cualquiera:\n  \"ACCIÓN 1 DEL 70%:\"\n  \"ACCIÓN 2 DEL 70%:\"\n  \"ACCIÓN 3 DEL 70%:\"\n\n  ACCION20_O_10_VISIBLE=true si C_NORM contiene cualquiera:\n  \"ACCIÓN 1 DEL 20%:\"\n  \"ACCIÓN 2 DEL 20%:\"\n  \"RECURSO 1 DEL 10%:\"\n  \"RECURSO 2 DEL 10%:\"\n\n  Si ACCION20_O_10_VISIBLE=true:\n    devuelve BASE EXACTO. FIN ABSOLUTO.\n\n  Si ACCION70_VISIBLE=true:\n    TITULO_A2=texto de la misma línea después del marcador visible 70%, usando LIMPIA_TITULO.\n    OUT=BASE exacto.\n\n    Si NEGATIVO:\n      OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO_A2)\n      OUT.etapa=\"A2_70\"\n      devuelve OUT. FIN ABSOLUTO.\n\n    Si AFIRMATIVO:\n      si CANON(TITULO_A2)==CANON(BASE.A1_70)\n      o CANON(TITULO_A2) existe en CANON(BASE.rechazadas70):\n        devuelve BASE EXACTO. FIN ABSOLUTO.\n\n      OUT.A2_70=TITULO_A2\n      OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO_A2)\n      OUT.etapa=\"A3_70\"\n      devuelve OUT. FIN ABSOLUTO.\n\n  devuelve BASE EXACTO. FIN ABSOLUTO.\n\nCONTROL CRÍTICO:\nBASE:\nA1_70=\"Motiva...\"\nA2_70=null\nA1_20=null\nA2_20=null\netapa=\"A2_70\"\n\nCAMILA=\"ACCIÓN 2 DEL 20%: Pide a su jefe...\"\nUSUARIO=\"si\"\n\n=> devuelve BASE EXACTO.\n=> A2_20 DEBE seguir null.\n=> A1_20 DEBE seguir null.\n=> etapa DEBE seguir \"A2_70\".\n\n====================================================================\n2.45 FIREWALL A1_20 — OBLIGATORIO\n====================================================================\n\nSi BASE.A1_70!=null\nY BASE.A2_70!=null\nY BASE.A1_20==null:\n\n  ACCION20_VISIBLE=true si C_NORM contiene:\n  \"ACCIÓN 1 DEL 20%:\" o \"ACCIÓN 2 DEL 20%:\".\n\n  Si ACCION20_VISIBLE=true:\n    TITULO_A1_20=texto de la misma línea después del marcador visible 20%, usando LIMPIA_TITULO.\n    OUT=BASE exacto.\n\n    Si NEGATIVO:\n      OUT.A1_20=null\n      OUT.A2_20=BASE.A2_20\n      OUT.rechazadas20=AGREGA_UNICO(BASE.rechazadas20,TITULO_A1_20)\n      OUT.etapa=\"A1_20\"\n      devuelve OUT. FIN ABSOLUTO.\n\n    Si AFIRMATIVO o SOLO:\n      OUT.A1_20=TITULO_A1_20\n      OUT.A2_20=BASE.A2_20\n      OUT.rechazadas20=REMOVE_SOLO(BASE.rechazadas20,TITULO_A1_20)\n      si SOLO: OUT.etapa=\"R1_10\"\n      si no: OUT.etapa=\"A2_20\"\n      devuelve OUT. FIN ABSOLUTO.\n\n    devuelve BASE EXACTO. FIN ABSOLUTO.\n\nCONTROL CRÍTICO:\nBASE.A1_20=null\nBASE.rechazadas20=[\"Pide feedback...\"]\nBASE.etapa=\"A1_20\"\nCAMILA=\"ACCIÓN 2 DEL 20%: Solicita lluvia de ideas...\"\nUSUARIO=\"si\"\n=> OUT.A1_20=\"Solicita lluvia de ideas...\"\n=> OUT.A2_20 sigue null\n=> rechazadas20 conserva \"Pide feedback...\"\n=> etapa=\"A2_20\".\n\n====================================================================\n2.46 FIREWALL A2_20 — RECHAZO/ACEPTACIÓN TEMPRANA\n====================================================================\n\nSi BASE.etapa==\"A2_20\"\nY BASE.A1_20!=null\nY BASE.A2_20==null\nY C_NORM contiene \"ACCIÓN 2 DEL 20%:\":\n\n  TITULO_A2_20=texto de la misma línea después de \"ACCIÓN 2 DEL 20%:\", usando LIMPIA_TITULO.\n  OUT=BASE exacto.\n\n  Si NEGATIVO:\n    OUT.rechazadas20=AGREGA_UNICO(BASE.rechazadas20,TITULO_A2_20)\n    OUT.etapa=\"A2_20\"\n    devuelve OUT. FIN ABSOLUTO.\n\n  Si SOLO:\n    OUT.etapa=\"R1_10\"\n    devuelve OUT. FIN ABSOLUTO.\n\n  Si AFIRMATIVO:\n    si CANON(TITULO_A2_20)==CANON(BASE.A1_20)\n    o CANON(TITULO_A2_20) existe en CANON(BASE.rechazadas20):\n      devuelve BASE EXACTO. FIN ABSOLUTO.\n    OUT.A2_20=TITULO_A2_20\n    OUT.rechazadas20=REMOVE_SOLO(BASE.rechazadas20,TITULO_A2_20)\n    OUT.etapa=\"R1_10\"\n    devuelve OUT. FIN ABSOLUTO.\n\n  devuelve BASE EXACTO. FIN ABSOLUTO.\n\nCONTROL:\nA1_20=\"Asesorar...\"\nA2_20=null\nrechazadas20=[]\nCAMILA=\"ACCIÓN 2 DEL 20%: Coordinar...\"\nUSUARIO=\"no\"\n=> A2_20=null\n=> rechazadas20=[\"Coordinar...\"]\n=> etapa=A2_20\n=> Coordinar NO puede volver a presentarse.\n\n====================================================================\n2.5 FIREWALL R1_10 — PRESERVACIÓN DE RECHAZADOS\n====================================================================\n\nR1_VISIBLE=true si C_NORM contiene:\n\"RECURSO 1 DEL 10%:\"\ny \"¿Confirmas que quieres aceptar el RECURSO 1 DEL 10%?\"\n\nSi R1_VISIBLE=true y BASE.etapa in [\"R1_10\",\"A2_20\"] y BASE.R1_10==null:\n\n TITULO_R1=texto de la misma línea después de \"RECURSO 1 DEL 10%:\", usando LIMPIA_TITULO.\n URL_R1=primera URL http/https asociada al recurso.\n\n Si NEGATIVO:\n   OUT=BASE exacto.\n   OUT.rechazados10=AGREGA_UNICO(BASE.rechazados10,TITULO_R1).\n   OUT.etapa=\"R1_10\".\n   devuelve OUT. FIN ABSOLUTO.\n\n Si AFIRMATIVO o SOLO:\n   OUT=BASE exacto.\n   OUT.R1_10=TITULO_R1.\n   OUT.enlaceR1_10=URL_R1.\n\n   # REGLA CRÍTICA:\n   # aceptar un recurso distinto NO borra rechazados anteriores.\n   OUT.rechazados10=copia EXACTA BASE.rechazados10.\n   Si CANON(TITULO_R1) existe en CANON(BASE.rechazados10):\n      elimina SOLO TITULO_R1 de OUT.rechazados10.\n\n   OUT.etapa=\"R2_10\".\n   devuelve OUT. FIN ABSOLUTO.\n\nCONTROL OBLIGATORIO:\nBASE.rechazados10=[\"Curso: Gestión de procesos y mejora continua – EdX.\"]\nTITULO_R1=\"Taller: Productividad para líderes de equipos – Udemy.\"\nUSUARIO=\"si\"\n=> R1_10=Udemy\n=> rechazados10=[\"Curso: Gestión de procesos y mejora continua – EdX.\"]\n=> EdX NO puede quedar libre otra vez.\n\n====================================================================\n3. RESUMEN — PRIORIDAD MÁXIMA Y CERRADA\n====================================================================\n\nRESUMEN=true si C_NORM contiene exactamente la pregunta:\n\"¿Te parece alcanzable este plan?\"\n\nSI RESUMEN=true:\n- OUT=BASE exacto.\n- NO evalúes fechas, acciones, recursos ni otras ramas.\n\nCONFIRMACION_RESUMEN=true SOLO si u, después de trim y quitar .,! finales, es EXACTAMENTE uno de:\n\"si\",\"sí\",\"acepto\",\"dale\",\"de una\",\"perfecto\",\"claro\",\"me gusta\",\n\"si me gusta\",\"sí me gusta\",\"lo acepto\",\"la acepto\".\n\nSi CONFIRMACION_RESUMEN=true:\n OUT.etapa=\"FECHAS\".\n devuelve OUT. FIN ABSOLUTO.\n\nPara CUALQUIER otro USUARIO:\n OUT.etapa=\"RESUMEN_FINAL\".\n devuelve OUT. FIN ABSOLUTO.\n\nCONTROLES:\nUSUARIO=\"el 15 de septiembre\" => RESUMEN_FINAL.\nUSUARIO=\"15/09/2026\" => RESUMEN_FINAL.\nUSUARIO=\"no\" => RESUMEN_FINAL.\nUSUARIO=\"averiguar cosas nuevas\" => RESUMEN_FINAL.\nUSUARIO=\"si\" => FECHAS.\nUSUARIO=\"sí\" => FECHAS.\n\nNINGÚN slot, enlace o lista puede cambiar en esta rama.\n\n====================================================================\n4. EXTRAER PROPUESTA\n====================================================================\n\nDetecta en C_NORM el marcador visible:\nM1=\"ACCIÓN 1 DEL 70%:\"\nM2=\"ACCIÓN 2 DEL 70%:\"\nM3=\"ACCIÓN 3 DEL 70%:\"\nM4=\"ACCIÓN 1 DEL 20%:\"\nM5=\"ACCIÓN 2 DEL 20%:\"\nM6=\"RECURSO 1 DEL 10%:\"\nM7=\"RECURSO 2 DEL 10%:\"\n\nTITULO = texto de la misma línea después del marcador detectado, usando LIMPIA_TITULO.\n\nPara M6/M7:\nURL = primera URL http/https asociada a ese recurso antes de la pregunta.\nSi está como [texto](URL), usa URL real.\n\nSi no hay marcador de confirmación reconocido:\ndevuelve BASE. FIN.\n\n====================================================================\n5. SWITCH DETERMINISTA\n====================================================================\n\n------------------------------------------------------------\n5.1 A1_70\n------------------------------------------------------------\n\nAPLICA si:\nBASE.etapa==\"A1_70\"\nY BASE.A1_70==null\nY C contiene confirmación de CUALQUIER acción del 70%.\nEl rótulo visible NO puede desplazar el slot.\n\nOUT=BASE exacto.\n\nSi NEGATIVO:\n OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO)\n OUT.etapa=\"A1_70\"\n devuelve OUT. FIN.\n\nSi AFIRMATIVO:\n OUT.A1_70=TITULO\n OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO)\n OUT.etapa=\"A2_70\"\n devuelve OUT. FIN.\n\n------------------------------------------------------------\n5.2 A2_70\n------------------------------------------------------------\n\nAPLICA si:\nBASE.etapa==\"A2_70\"\nY BASE.A1_70!=null\nY BASE.A2_70==null\nY C contiene confirmación de acción 70%.\n\nOUT=BASE exacto.\n\nSi NEGATIVO:\n OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO)\n OUT.etapa=\"A2_70\"\n devuelve OUT. FIN.\n\nSi AFIRMATIVO:\n si CANON(TITULO)==CANON(BASE.A1_70) o CANON(TITULO) existe en CANON(BASE.rechazadas70):\n   devuelve BASE. FIN.\n OUT.A2_70=TITULO\n OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO)\n OUT.etapa=\"A3_70\"\n devuelve OUT. FIN.\n\n------------------------------------------------------------\n5.3 A3_70\n------------------------------------------------------------\n\nAPLICA si:\nBASE.etapa==\"A3_70\"\nY BASE.A1_70!=null\nY BASE.A2_70!=null\nY BASE.A3_70==null\nY C contiene confirmación de acción 70%.\n\nOUT=BASE exacto.\n\nSi OMITIR_A3:\n OUT.etapa=\"A1_20\"\n devuelve OUT. FIN.\n\nSi NEGATIVO:\n OUT.rechazadas70=AGREGA_UNICO(BASE.rechazadas70,TITULO)\n OUT.etapa=\"A3_70\"\n devuelve OUT. FIN.\n\nSi AFIRMATIVO:\n si CANON(TITULO) está en CANON([BASE.A1_70,BASE.A2_70]+BASE.rechazadas70):\n   OUT.etapa=\"A1_20\"\n   devuelve OUT. FIN.\n OUT.A3_70=TITULO\n OUT.rechazadas70=REMOVE_SOLO(BASE.rechazadas70,TITULO)\n OUT.etapa=\"A1_20\"\n devuelve OUT. FIN.\n\n------------------------------------------------------------\n5.4 A1_20\n------------------------------------------------------------\n\nAPLICA si:\nBASE.etapa en [\"A1_20\",\"A3_70\"]\nY BASE.A1_70!=null\nY BASE.A2_70!=null\nY BASE.A1_20==null\nY C contiene confirmación de \"ACCIÓN 1 DEL 20%\".\n\nOUT=BASE exacto.\n\nSi NEGATIVO:\n OUT.rechazadas20=AGREGA_UNICO(BASE.rechazadas20,TITULO)\n OUT.etapa=\"A1_20\"\n devuelve OUT. FIN.\n\nSi AFIRMATIVO o SOLO:\n OUT.A1_20=TITULO\n OUT.rechazadas20=REMOVE_SOLO(BASE.rechazadas20,TITULO)\n si SOLO: OUT.etapa=\"R1_10\"\n si no: OUT.etapa=\"A2_20\"\n devuelve OUT. FIN.\n\n------------------------------------------------------------\n5.5 A2_20\n------------------------------------------------------------\n\nAPLICA si:\nBASE.etapa==\"A2_20\"\nY BASE.A1_20!=null\nY BASE.A2_20==null\nY C contiene confirmación de \"ACCIÓN 2 DEL 20%\".\n\nOUT=BASE exacto.\nIMPORTANTE: OUT.A1_20 permanece EXACTAMENTE BASE.A1_20 en TODA esta rama.\n\nSi NEGATIVO:\n OUT.rechazadas20=AGREGA_UNICO(BASE.rechazadas20,TITULO)\n OUT.etapa=\"A2_20\"\n devuelve OUT. FIN.\n\nSi SOLO:\n OUT.etapa=\"R1_10\"\n devuelve OUT. FIN.\n\nSi AFIRMATIVO:\n si CANON(TITULO)==CANON(BASE.A1_20) o CANON(TITULO) existe en CANON(BASE.rechazadas20):\n   devuelve BASE. FIN.\n OUT.A2_20=TITULO\n OUT.rechazadas20=REMOVE_SOLO(BASE.rechazadas20,TITULO)\n OUT.etapa=\"R1_10\"\n devuelve OUT. FIN.\n\n------------------------------------------------------------\n5.6 R1_10\n------------------------------------------------------------\n\nAPLICA si:\nBASE.etapa en [\"R1_10\",\"A2_20\"]\nY BASE.A1_20!=null\nY BASE.R1_10==null\nY C contiene confirmación de \"RECURSO 1 DEL 10%\".\n\nOUT=BASE exacto.\nIMPORTANTE:\nOUT.A1_20=BASE.A1_20\nOUT.A2_20=BASE.A2_20\nOUT.rechazadas20=BASE.rechazadas20\n\nSi NEGATIVO:\n OUT.rechazados10=AGREGA_UNICO(BASE.rechazados10,TITULO)\n OUT.etapa=\"R1_10\"\n devuelve OUT. FIN.\n\nSi AFIRMATIVO o SOLO:\n OUT.R1_10=TITULO\n OUT.enlaceR1_10=URL\n si CANON(TITULO) existe en CANON(BASE.rechazados10):\n   OUT.rechazados10=REMOVE_SOLO(BASE.rechazados10,TITULO)\n si CANON(TITULO) NO existe en CANON(BASE.rechazados10):\n   OUT.rechazados10=copia EXACTA BASE.rechazados10\n OUT.etapa=\"R2_10\"\n devuelve OUT. FIN.\n\nCONTROL OBLIGATORIO:\nBASE.rechazados10=[EdX], TITULO=Udemy, AFIRMATIVO\n=> OUT.rechazados10=[EdX].\nUdemy NO puede borrar EdX.\n\n------------------------------------------------------------\n5.7 R2_10\n------------------------------------------------------------\n\nAPLICA si:\nBASE.etapa==\"R2_10\"\nY BASE.R1_10!=null\nY BASE.R2_10==null\nY C contiene confirmación de \"RECURSO 2 DEL 10%\".\n\nOUT=BASE exacto.\nIMPORTANTE: todos los slots 70/20 y R1 permanecen BASE.\n\nSi NEGATIVO o SOLO:\n OUT.rechazados10=AGREGA_UNICO(BASE.rechazados10,TITULO) SOLO si NEGATIVO\n OUT.etapa=\"RESUMEN_FINAL\"\n devuelve OUT. FIN.\n\nSi AFIRMATIVO:\n si CANON(TITULO)==CANON(BASE.R1_10):\n   OUT.etapa=\"RESUMEN_FINAL\"\n   devuelve OUT. FIN.\n si CANON(TITULO) existe en CANON(BASE.rechazados10):\n   # Un recurso rechazado como R1 jamás puede revivir como R2.\n   OUT.R2_10=null\n   OUT.enlaceR2_10=null\n   OUT.rechazados10=copia EXACTA BASE.rechazados10\n   OUT.etapa=\"RESUMEN_FINAL\"\n   devuelve OUT. FIN ABSOLUTO.\n OUT.R2_10=TITULO\n OUT.enlaceR2_10=URL\n OUT.rechazados10=REMOVE_SOLO(BASE.rechazados10,TITULO)\n OUT.etapa=\"RESUMEN_FINAL\"\n devuelve OUT. FIN.\n\n====================================================================\n6. SALIDA SIN RAMA\n====================================================================\n\nDevuelve BASE exacto.\nFIN.\n\n====================================================================\n7. INVARIANTES FINALES OBLIGATORIAS\n====================================================================\n\nAntes de devolver cualquier OUT verifica:\n\n1. Si BASE.A1_20!=null y la rama NO es 5.4:\n   OUT.A1_20 DEBE ser exactamente BASE.A1_20.\n\n2. Si BASE.A1_70!=null y la rama no es 5.1:\n   OUT.A1_70=BASE.A1_70.\n\n3. Si BASE.A2_70!=null y la rama no es 5.2:\n   OUT.A2_70=BASE.A2_70.\n\n4. Si BASE.A3_70!=null y la rama no es 5.3:\n   OUT.A3_70=BASE.A3_70.\n\n5. Toda rama 10%:\n   OUT.A1_20=BASE.A1_20\n   OUT.A2_20=BASE.A2_20\n   OUT.rechazadas20=BASE.rechazadas20.\n\n6. Para CADA x en BASE.rechazados10:\n   si CANON(x)!=CANON(TITULO aceptado):\n      x DEBE seguir en OUT.rechazados10.\n   Si TITULO aceptado NO estaba en BASE.rechazados10:\n      OUT.rechazados10 DEBE ser IDÉNTICO a BASE.rechazados10.\n\n7. Nunca puede existir:\n   A1_20==null y A2_20!=null.\n   Si BASE ya cumple la jerarquía, ninguna rama puede romperla.\n\n8. Exactamente 13 claves. Sin Markdown. Sin explicaciones.\n\nCASO DE REGRESIÓN CRÍTICO:\nBASE:\nA1_20=\"Sesión quincenal...\"\nA2_20=null\nrechazadas20=[]\netapa=\"A2_20\"\n\nCAMILA=\"ACCIÓN 2 DEL 20%: Asesorar...\"\nUSUARIO=\"no\"\n\n=> A1_20 sigue \"Sesión quincenal...\"\n=> A2_20=null\n=> rechazadas20=[\"Asesorar...\"]\n=> etapa=\"A2_20\"\n\nSiguiente:\nCAMILA=\"ACCIÓN 2 DEL 20%: Coordinar...\"\nUSUARIO=\"si\"\n\n=> A1_20 sigue \"Sesión quincenal...\"\n=> A2_20=\"Coordinar...\"\n=> etapa=\"R1_10\"\n\nFIN DEL PROMPT.\n","temperature":0.5}
```

### Generador · extractor_acciones

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 02. Mensaje2 (al entrar); 02.1. Conversacional Evaluacion Desempeño › 03. Contexto sobre fechas (al entrar); 03.1. Conversacional Lider Misión › 02. Mensaje2 (al entrar); 03.1. Conversacional Lider Misión › 03. Contexto sobre fechas (al entrar)

Prompt:

> Eres un extractor determinista de ítems aceptados de un Plan de Desarrollo Individual 70/20/10.
> 
> Tu única tarea es analizar el siguiente estado JSON:
> 
> $estado_pdi
> 
> El objeto puede contener estos campos:
> 
> A1_70
> A2_70
> A3_70
> A1_20
> A2_20
> R1_10
> enlaceR1_10
> R2_10
> enlaceR2_10
> rechazadas70
> rechazadas20
> rechazados10
> etapa
> 
> Debes devolver exclusivamente los ítems ACEPTADOS que requieren una fecha de compromiso.
> 
> ============================================================
> ORDEN OBLIGATORIO
> ============================================================
> 
> Evalúa los campos exactamente en este orden:
> 
> 1. A1_70
> 2. A2_70
> 3. A3_70
> 4. A1_20
> 5. A2_20
> 6. R1_10
> 7. R2_10
> 
> Este orden es obligatorio.
> 
> NO cambies el orden aunque existan campos null entre ellos.
> 
> Los campos null simplemente se omiten.
> 
> ============================================================
> REGLAS ABSOLUTAS
> ============================================================
> 
> 1. Si uno de estos campos contiene un texto distinto de null, inclúyelo en el arreglo:
> 
> A1_70
> A2_70
> A3_70
> A1_20
> A2_20
> R1_10
> R2_10
> 
> 2. Si uno de esos campos es null, omítelo completamente.
> 
> 3. Nunca insertes null en el arreglo de salida.
> 
> 4. Nunca inventes un valor para un campo null.
> 
> 5. Nunca completes slots faltantes.
> 
> 6. Nunca reconstruyas una acción o recurso desde:
> - conversaciones anteriores;
> - rechazos;
> - propuestas;
> - resúmenes;
> - otros parámetros.
> 
> 7. Nunca uses etapa para decidir qué elementos incluir.
> 
> 8. Nunca incluyas en el arreglo:
> 
> - enlaceR1_10
> - enlaceR2_10
> - rechazadas70
> - rechazadas20
> - rechazados10
> - etapa
> 
> 9. Los recursos R1_10 y R2_10 SÍ requieren fecha de compromiso.
> 
> Por tanto:
> 
> R1_10 distinto de null
> → incluir R1_10.
> 
> R2_10 distinto de null
> → incluir R2_10.
> 
> 10. Conserva literalmente el texto de cada valor.
> 
> No resumas.
> No corrijas.
> No reformules.
> No traduzcas.
> No cambies mayúsculas.
> No cambies puntuación.
> No agregues ni elimines palabras.
> 
> 11. La cantidad final del arreglo debe ser EXACTAMENTE igual a la cantidad de campos no nulos entre:
> 
> A1_70
> A2_70
> A3_70
> A1_20
> A2_20
> R1_10
> R2_10
> 
> 12. La cantidad puede variar según el plan construido.
> 
> De acuerdo con la metodología actual:
> 
> 70%:
> mínimo 2
> máximo 3
> 
> 20%:
> mínimo 1
> máximo 2
> 
> 10%:
> mínimo 1
> máximo 2
> 
> Por lo tanto, el arreglo final normalmente puede contener entre 4 y 7 elementos.
> 
> NO fuerces una cantidad fija.
> 
> La fuente de verdad es únicamente la cantidad real de campos no nulos.
> 
> ============================================================
> EJEMPLO 1 — DOS RECURSOS
> ============================================================
> 
> Si recibes:
> 
> {
>   "A1_70": "Acción uno",
>   "A2_70": "Acción dos",
>   "A3_70": null,
>   "A1_20": "Acción tres",
>   "A2_20": null,
>   "R1_10": "Curso uno",
>   "enlaceR1_10": "https://ejemplo1.com",
>   "R2_10": "Curso dos",
>   "enlaceR2_10": "https://ejemplo2.com",
>   "rechazadas70": [],
>   "rechazadas20": [],
>   "rechazados10": [],
>   "etapa": "FIN_PDI"
> }
> 
> debes devolver exactamente:
> 
> ["Acción uno","Acción dos","Acción tres","Curso uno","Curso dos"]
> 
> Cantidad final:
> 
> 5
> 
> ============================================================
> EJEMPLO 2 — UN SOLO RECURSO
> ============================================================
> 
> Si recibes:
> 
> {
>   "A1_70": "Acción uno",
>   "A2_70": "Acción dos",
>   "A3_70": null,
>   "A1_20": "Acción tres",
>   "A2_20": null,
>   "R1_10": "Curso uno",
>   "enlaceR1_10": "https://ejemplo1.com",
>   "R2_10": null,
>   "enlaceR2_10": null,
>   "rechazadas70": [],
>   "rechazadas20": [],
>   "rechazados10": [],
>   "etapa": "FIN_PDI"
> }
> 
> debes devolver exactamente:
> 
> ["Acción uno","Acción dos","Acción tres","Curso uno"]
> 
> Cantidad final:
> 
> 4
> 
> ============================================================
> EJEMPLO 3 — PLAN MÁXIMO
> ============================================================
> 
> Si recibes:
> 
> {
>   "A1_70": "Acción 70 uno",
>   "A2_70": "Acción 70 dos",
>   "A3_70": "Acción 70 tres",
>   "A1_20": "Acción 20 uno",
>   "A2_20": "Acción 20 dos",
>   "R1_10": "Recurso uno",
>   "enlaceR1_10": "https://ejemplo1.com",
>   "R2_10": "Recurso dos",
>   "enlaceR2_10": "https://ejemplo2.com",
>   "rechazadas70": [],
>   "rechazadas20": [],
>   "rechazados10": [],
>   "etapa": "FIN_PDI"
> }
> 
> debes devolver exactamente:
> 
> ["Acción 70 uno","Acción 70 dos","Acción 70 tres","Acción 20 uno","Acción 20 dos","Recurso uno","Recurso dos"]
> 
> Cantidad final:
> 
> 7
> 
> ============================================================
> VALIDACIÓN FINAL OBLIGATORIA
> ============================================================
> 
> Antes de responder verifica uno por uno:
> 
> A1_70:
> - si no es null → incluido;
> - si es null → omitido.
> 
> A2_70:
> - si no es null → incluido;
> - si es null → omitido.
> 
> A3_70:
> - si no es null → incluido;
> - si es null → omitido.
> 
> A1_20:
> - si no es null → incluido;
> - si es null → omitido.
> 
> A2_20:
> - si no es null → incluido;
> - si es null → omitido.
> 
> R1_10:
> - si no es null → incluido;
> - si es null → omitido.
> 
> R2_10:
> - si no es null → incluido;
> - si es null → omitido.
> 
> Después confirma:
> 
> - no falta ningún campo no nulo;
> - no agregaste ningún campo null;
> - R2_10 fue evaluado;
> - enlaceR1_10 no fue incluido;
> - enlaceR2_10 no fue incluido;
> - rechazadas70 no fue incluido;
> - rechazadas20 no fue incluido;
> - rechazados10 no fue incluido;
> - etapa no fue incluida;
> - el orden final respeta:
>   A1_70,
>   A2_70,
>   A3_70,
>   A1_20,
>   A2_20,
>   R1_10,
>   R2_10.
> 
> ============================================================
> FORMATO DE SALIDA
> ============================================================
> 
> Devuelve exclusivamente un arreglo JSON plano de textos.
> 
> Debe ser JSON válido.
> 
> Debe estar en una sola línea.
> 
> No agregues:
> - explicaciones;
> - introducciones;
> - Markdown;
> - bloques ```json;
> - propiedades adicionales;
> - cantidad;
> - texto antes o después del arreglo.
> 
> Devuelve únicamente el arreglo JSON.

```json camila:generator
{"name":"extractor_acciones","id":"80a735b9-a4cb-4ed7-b295-27d78d7ba5ff","prompt":"Eres un extractor determinista de ítems aceptados de un Plan de Desarrollo Individual 70/20/10.\n\nTu única tarea es analizar el siguiente estado JSON:\n\n$estado_pdi\n\nEl objeto puede contener estos campos:\n\nA1_70\nA2_70\nA3_70\nA1_20\nA2_20\nR1_10\nenlaceR1_10\nR2_10\nenlaceR2_10\nrechazadas70\nrechazadas20\nrechazados10\netapa\n\nDebes devolver exclusivamente los ítems ACEPTADOS que requieren una fecha de compromiso.\n\n============================================================\nORDEN OBLIGATORIO\n============================================================\n\nEvalúa los campos exactamente en este orden:\n\n1. A1_70\n2. A2_70\n3. A3_70\n4. A1_20\n5. A2_20\n6. R1_10\n7. R2_10\n\nEste orden es obligatorio.\n\nNO cambies el orden aunque existan campos null entre ellos.\n\nLos campos null simplemente se omiten.\n\n============================================================\nREGLAS ABSOLUTAS\n============================================================\n\n1. Si uno de estos campos contiene un texto distinto de null, inclúyelo en el arreglo:\n\nA1_70\nA2_70\nA3_70\nA1_20\nA2_20\nR1_10\nR2_10\n\n2. Si uno de esos campos es null, omítelo completamente.\n\n3. Nunca insertes null en el arreglo de salida.\n\n4. Nunca inventes un valor para un campo null.\n\n5. Nunca completes slots faltantes.\n\n6. Nunca reconstruyas una acción o recurso desde:\n- conversaciones anteriores;\n- rechazos;\n- propuestas;\n- resúmenes;\n- otros parámetros.\n\n7. Nunca uses etapa para decidir qué elementos incluir.\n\n8. Nunca incluyas en el arreglo:\n\n- enlaceR1_10\n- enlaceR2_10\n- rechazadas70\n- rechazadas20\n- rechazados10\n- etapa\n\n9. Los recursos R1_10 y R2_10 SÍ requieren fecha de compromiso.\n\nPor tanto:\n\nR1_10 distinto de null\n→ incluir R1_10.\n\nR2_10 distinto de null\n→ incluir R2_10.\n\n10. Conserva literalmente el texto de cada valor.\n\nNo resumas.\nNo corrijas.\nNo reformules.\nNo traduzcas.\nNo cambies mayúsculas.\nNo cambies puntuación.\nNo agregues ni elimines palabras.\n\n11. La cantidad final del arreglo debe ser EXACTAMENTE igual a la cantidad de campos no nulos entre:\n\nA1_70\nA2_70\nA3_70\nA1_20\nA2_20\nR1_10\nR2_10\n\n12. La cantidad puede variar según el plan construido.\n\nDe acuerdo con la metodología actual:\n\n70%:\nmínimo 2\nmáximo 3\n\n20%:\nmínimo 1\nmáximo 2\n\n10%:\nmínimo 1\nmáximo 2\n\nPor lo tanto, el arreglo final normalmente puede contener entre 4 y 7 elementos.\n\nNO fuerces una cantidad fija.\n\nLa fuente de verdad es únicamente la cantidad real de campos no nulos.\n\n============================================================\nEJEMPLO 1 — DOS RECURSOS\n============================================================\n\nSi recibes:\n\n{\n  \"A1_70\": \"Acción uno\",\n  \"A2_70\": \"Acción dos\",\n  \"A3_70\": null,\n  \"A1_20\": \"Acción tres\",\n  \"A2_20\": null,\n  \"R1_10\": \"Curso uno\",\n  \"enlaceR1_10\": \"https://ejemplo1.com\",\n  \"R2_10\": \"Curso dos\",\n  \"enlaceR2_10\": \"https://ejemplo2.com\",\n  \"rechazadas70\": [],\n  \"rechazadas20\": [],\n  \"rechazados10\": [],\n  \"etapa\": \"FIN_PDI\"\n}\n\ndebes devolver exactamente:\n\n[\"Acción uno\",\"Acción dos\",\"Acción tres\",\"Curso uno\",\"Curso dos\"]\n\nCantidad final:\n\n5\n\n============================================================\nEJEMPLO 2 — UN SOLO RECURSO\n============================================================\n\nSi recibes:\n\n{\n  \"A1_70\": \"Acción uno\",\n  \"A2_70\": \"Acción dos\",\n  \"A3_70\": null,\n  \"A1_20\": \"Acción tres\",\n  \"A2_20\": null,\n  \"R1_10\": \"Curso uno\",\n  \"enlaceR1_10\": \"https://ejemplo1.com\",\n  \"R2_10\": null,\n  \"enlaceR2_10\": null,\n  \"rechazadas70\": [],\n  \"rechazadas20\": [],\n  \"rechazados10\": [],\n  \"etapa\": \"FIN_PDI\"\n}\n\ndebes devolver exactamente:\n\n[\"Acción uno\",\"Acción dos\",\"Acción tres\",\"Curso uno\"]\n\nCantidad final:\n\n4\n\n============================================================\nEJEMPLO 3 — PLAN MÁXIMO\n============================================================\n\nSi recibes:\n\n{\n  \"A1_70\": \"Acción 70 uno\",\n  \"A2_70\": \"Acción 70 dos\",\n  \"A3_70\": \"Acción 70 tres\",\n  \"A1_20\": \"Acción 20 uno\",\n  \"A2_20\": \"Acción 20 dos\",\n  \"R1_10\": \"Recurso uno\",\n  \"enlaceR1_10\": \"https://ejemplo1.com\",\n  \"R2_10\": \"Recurso dos\",\n  \"enlaceR2_10\": \"https://ejemplo2.com\",\n  \"rechazadas70\": [],\n  \"rechazadas20\": [],\n  \"rechazados10\": [],\n  \"etapa\": \"FIN_PDI\"\n}\n\ndebes devolver exactamente:\n\n[\"Acción 70 uno\",\"Acción 70 dos\",\"Acción 70 tres\",\"Acción 20 uno\",\"Acción 20 dos\",\"Recurso uno\",\"Recurso dos\"]\n\nCantidad final:\n\n7\n\n============================================================\nVALIDACIÓN FINAL OBLIGATORIA\n============================================================\n\nAntes de responder verifica uno por uno:\n\nA1_70:\n- si no es null → incluido;\n- si es null → omitido.\n\nA2_70:\n- si no es null → incluido;\n- si es null → omitido.\n\nA3_70:\n- si no es null → incluido;\n- si es null → omitido.\n\nA1_20:\n- si no es null → incluido;\n- si es null → omitido.\n\nA2_20:\n- si no es null → incluido;\n- si es null → omitido.\n\nR1_10:\n- si no es null → incluido;\n- si es null → omitido.\n\nR2_10:\n- si no es null → incluido;\n- si es null → omitido.\n\nDespués confirma:\n\n- no falta ningún campo no nulo;\n- no agregaste ningún campo null;\n- R2_10 fue evaluado;\n- enlaceR1_10 no fue incluido;\n- enlaceR2_10 no fue incluido;\n- rechazadas70 no fue incluido;\n- rechazadas20 no fue incluido;\n- rechazados10 no fue incluido;\n- etapa no fue incluida;\n- el orden final respeta:\n  A1_70,\n  A2_70,\n  A3_70,\n  A1_20,\n  A2_20,\n  R1_10,\n  R2_10.\n\n============================================================\nFORMATO DE SALIDA\n============================================================\n\nDevuelve exclusivamente un arreglo JSON plano de textos.\n\nDebe ser JSON válido.\n\nDebe estar en una sola línea.\n\nNo agregues:\n- explicaciones;\n- introducciones;\n- Markdown;\n- bloques \u0060\u0060\u0060json;\n- propiedades adicionales;\n- cantidad;\n- texto antes o después del arreglo.\n\nDevuelve únicamente el arreglo JSON.","temperature":0.5}
```

### Generador · extractor_materiales

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 02. Mensaje2 (al entrar); 02.1. Conversacional Evaluacion Desempeño › 03. Contexto sobre fechas (al entrar); 03.1. Conversacional Lider Misión › 02. Mensaje2 (al entrar); 03.1. Conversacional Lider Misión › 03. Contexto sobre fechas (al entrar)

Prompt:

> Analiza el siguiente texto:
> 
> "\$resumen"
> 
> REGLA DE SEGURIDAD ABSOLUTA: 
> Si el texto anterior NO contiene de forma explícita la sección "MATERIALES COMPLEMENTARIOS" o contiene la frase "DEFINIR FECHAS", responde únicamente un arreglo vacío: []
> 
> Si el texto SÍ contiene la sección de materiales, actúa como un extractor de datos ultra estricto. Ignora por completo las secciones de PRÁCTICA, MENTORÍA, FORMACIÓN y META. Extrae únicamente los elementos bajo el título "MATERIALES COMPLEMENTARIOS" (como la película/documental y el podcast).
> 
> Devuelve EXCLUSIVAMENTE un arreglo JSON con el formato del siguiente ejemplo, sin explicaciones, sin introducciones y sin usar marcas de código como ```json:
> 
> [
>   {
>     "titulo": "Cadena de favores",
>     "enlace": "https://primevideo.com..."
>   },
>   {
>     "titulo": "Ser influyente",
>     "enlace": "https://spotify.com..."
>   }
> ]
> 

```json camila:generator
{"name":"extractor_materiales","id":"ff8d8a17-f4d9-4b41-98ec-f272798ffe60","prompt":"Analiza el siguiente texto:\n\n\"\\$resumen\"\n\nREGLA DE SEGURIDAD ABSOLUTA: \nSi el texto anterior NO contiene de forma explícita la sección \"MATERIALES COMPLEMENTARIOS\" o contiene la frase \"DEFINIR FECHAS\", responde únicamente un arreglo vacío: []\n\nSi el texto SÍ contiene la sección de materiales, actúa como un extractor de datos ultra estricto. Ignora por completo las secciones de PRÁCTICA, MENTORÍA, FORMACIÓN y META. Extrae únicamente los elementos bajo el título \"MATERIALES COMPLEMENTARIOS\" (como la película/documental y el podcast).\n\nDevuelve EXCLUSIVAMENTE un arreglo JSON con el formato del siguiente ejemplo, sin explicaciones, sin introducciones y sin usar marcas de código como \u0060\u0060\u0060json:\n\n[\n  {\n    \"titulo\": \"Cadena de favores\",\n    \"enlace\": \"https://primevideo.com...\"\n  },\n  {\n    \"titulo\": \"Ser influyente\",\n    \"enlace\": \"https://spotify.com...\"\n  }\n]\n","temperature":0.5}
```

### Generador · extraer_accion_fecha_actual

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente objeto JSON:
> 
> $jsonFechas
> 
> Extrae exclusivamente el valor de la propiedad:
> 
> itemProcesado.accion
> 
> Si itemProcesado es null, devuelve exclusivamente:
> 
> VACIO
> 
> No agregues explicaciones.
> No agregues comillas.
> No uses Markdown.
> No modifiques el texto.

```json camila:generator
{"name":"extraer_accion_fecha_actual","id":"5abc13f7-24c0-4470-911a-696b26e3a30b","prompt":"Analiza el siguiente objeto JSON:\n\n$jsonFechas\n\nExtrae exclusivamente el valor de la propiedad:\n\nitemProcesado.accion\n\nSi itemProcesado es null, devuelve exclusivamente:\n\nVACIO\n\nNo agregues explicaciones.\nNo agregues comillas.\nNo uses Markdown.\nNo modifiques el texto.","temperature":0.5}
```

### Generador · extraer_contador_accion_fecha_actual

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente objeto JSON:
> 
> $jsonFechas
> 
> Extrae exclusivamente el valor numérico de la propiedad:
> 
> itemProcesado.contadorAcciones
> 
> Si itemProcesado es null, devuelve exclusivamente:
> 
> 0
> 
> No agregues explicaciones.
> No agregues texto adicional.

```json camila:generator
{"name":"extraer_contador_accion_fecha_actual","id":"4d625475-4a90-4279-b425-ab7fdb6b7f47","prompt":"Analiza el siguiente objeto JSON:\n\n$jsonFechas\n\nExtrae exclusivamente el valor numérico de la propiedad:\n\nitemProcesado.contadorAcciones\n\nSi itemProcesado es null, devuelve exclusivamente:\n\n0\n\nNo agregues explicaciones.\nNo agregues texto adicional.","temperature":0.5}
```

### Generador · extraer_estado_fechas

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente contenido:
> 
> $json_fechas
> 
> Tu función es determinar el estado REAL del proceso a partir del contenido de pdiFechas y fechaValida.
> 
> NO confíes ciegamente en la propiedad "estado" si contradice el arreglo pdiFechas.
> 
> Reglas en este orden:
> 
> 1. Si el contenido no es un objeto JSON válido, no contiene pdiFechas o contiene el texto "Lo siento pero no tengo información al respecto", devuelve exclusivamente:
> ERROR
> 
> 2. Si fechaValida es false, devuelve exclusivamente:
> REINTENTAR
> 
> 3. Si pdiFechas contiene al menos un objeto cuya propiedad fecha sea null, devuelve exclusivamente:
> PREGUNTAR
> 
> Esto aplica incluso si la propiedad "estado" dice "COMPLETO".
> 
> 4. Si pdiFechas contiene uno o más objetos y TODOS tienen fecha distinta de null, devuelve exclusivamente:
> COMPLETO
> 
> 5. Durante la inicialización, si fechaValida es null y existen objetos con fecha null, devuelve exclusivamente:
> PREGUNTAR
> 
> Valores permitidos:
> PREGUNTAR
> REINTENTAR
> COMPLETO
> ERROR
> 
> No agregues explicaciones.
> No agregues comillas.
> No uses Markdown.
> No agregues ningún otro texto.
> 

```json camila:generator
{"name":"extraer_estado_fechas","id":"df9773d0-0c3a-40b9-aac8-3128f367efb6","prompt":"Analiza el siguiente contenido:\n\n$json_fechas\n\nTu función es determinar el estado REAL del proceso a partir del contenido de pdiFechas y fechaValida.\n\nNO confíes ciegamente en la propiedad \"estado\" si contradice el arreglo pdiFechas.\n\nReglas en este orden:\n\n1. Si el contenido no es un objeto JSON válido, no contiene pdiFechas o contiene el texto \"Lo siento pero no tengo información al respecto\", devuelve exclusivamente:\nERROR\n\n2. Si fechaValida es false, devuelve exclusivamente:\nREINTENTAR\n\n3. Si pdiFechas contiene al menos un objeto cuya propiedad fecha sea null, devuelve exclusivamente:\nPREGUNTAR\n\nEsto aplica incluso si la propiedad \"estado\" dice \"COMPLETO\".\n\n4. Si pdiFechas contiene uno o más objetos y TODOS tienen fecha distinta de null, devuelve exclusivamente:\nCOMPLETO\n\n5. Durante la inicialización, si fechaValida es null y existen objetos con fecha null, devuelve exclusivamente:\nPREGUNTAR\n\nValores permitidos:\nPREGUNTAR\nREINTENTAR\nCOMPLETO\nERROR\n\nNo agregues explicaciones.\nNo agregues comillas.\nNo uses Markdown.\nNo agregues ningún otro texto.\n","temperature":0.5}
```

### Generador · extraer_fecha_compromiso_actual

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente objeto JSON:
> 
> $jsonFechas
> 
> Extrae exclusivamente el valor de la propiedad:
> 
> itemProcesado.fechaCompromiso
> 
> Si itemProcesado es null, devuelve exclusivamente:
> 
> VACIO
> 
> Conserva la fecha en formato YYYY-MM-DD.
> 
> No agregues explicaciones.
> No agregues comillas.
> No uses Markdown.

```json camila:generator
{"name":"extraer_fecha_compromiso_actual","id":"61fa3d1a-d496-453e-9c0b-416e1f19aa78","prompt":"Analiza el siguiente objeto JSON:\n\n$jsonFechas\n\nExtrae exclusivamente el valor de la propiedad:\n\nitemProcesado.fechaCompromiso\n\nSi itemProcesado es null, devuelve exclusivamente:\n\nVACIO\n\nConserva la fecha en formato YYYY-MM-DD.\n\nNo agregues explicaciones.\nNo agregues comillas.\nNo uses Markdown.","temperature":0.5}
```

### Generador · extraer_fecha_valida

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente objeto JSON:
> 
> $jsonFechas
> 
> Extrae exclusivamente el valor de la propiedad:
> 
> fechaValida
> 
> Devuelve únicamente uno de estos valores:
> 
> true
> false
> null
> 
> No agregues explicaciones.
> No agregues comillas.
> No agregues ningún otro texto.

```json camila:generator
{"name":"extraer_fecha_valida","id":"9ff50e81-202c-4867-9c8d-b86d943be553","prompt":"Analiza el siguiente objeto JSON:\n\n$jsonFechas\n\nExtrae exclusivamente el valor de la propiedad:\n\nfechaValida\n\nDevuelve únicamente uno de estos valores:\n\ntrue\nfalse\nnull\n\nNo agregues explicaciones.\nNo agregues comillas.\nNo agregues ningún otro texto.","temperature":0.5}
```

### Generador · extraer_json_fechas

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente objeto JSON:
> 
> $jsonFechas
> 
> Extrae exclusivamente el arreglo completo contenido en la propiedad pdiFechas.
> 
> Devuelve todos los objetos del arreglo, desde el primero hasta el último.
> 
> La respuesta debe ser un arreglo JSON completo, válido y cerrado.
> 
> Devuelve el JSON de forma compacta, en una sola línea, sin espacios ni saltos de línea innecesarios.
> 
> No cortes ningún objeto.
> No omitas elementos.
> No resumas las acciones.
> No modifiques los textos.
> No devuelvas el objeto principal.
> No uses Markdown.
> No agregues explicaciones ni texto adicional.

```json camila:generator
{"name":"extraer_json_fechas","id":"c5ea3253-c5df-408e-bf20-abc7b402ebbb","prompt":"Analiza el siguiente objeto JSON:\n\n$jsonFechas\n\nExtrae exclusivamente el arreglo completo contenido en la propiedad pdiFechas.\n\nDevuelve todos los objetos del arreglo, desde el primero hasta el último.\n\nLa respuesta debe ser un arreglo JSON completo, válido y cerrado.\n\nDevuelve el JSON de forma compacta, en una sola línea, sin espacios ni saltos de línea innecesarios.\n\nNo cortes ningún objeto.\nNo omitas elementos.\nNo resumas las acciones.\nNo modifiques los textos.\nNo devuelvas el objeto principal.\nNo uses Markdown.\nNo agregues explicaciones ni texto adicional.","temperature":0.5}
```

### Generador · extraer_mensaje_fechas

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.2 Procesar item y fecha (al entrar); 03.1. Conversacional Lider Misión › 03.2 Procesar item y fecha (al entrar)

Prompt:

> Analiza el siguiente objeto JSON:
> 
> $json_fechas
> 
> FECHA MÍNIMA PERMITIDA:
> 
> $fecha_minima
> 
> FECHA MÁXIMA PERMITIDA:
> 
> $fecha_maxima
> 
> Tu única función es determinar qué mensaje debe mostrarse al usuario.
> 
> La fuente de verdad para saber si queda un ítem pendiente es el arreglo pdiFechas.
> 
> ============================================================
> PRIORIDAD 1 — REINTENTO POR FECHA INVÁLIDA
> ============================================================
> 
> Si fechaValida es false o la propiedad estado es "REINTENTAR":
> 
> Devuelve exclusivamente el contenido de la propiedad "mensaje".
> 
> ============================================================
> PRIORIDAD 2 — TODAVÍA EXISTEN FECHAS NULL
> ============================================================
> 
> Si dentro de pdiFechas existe al menos un objeto cuya propiedad fecha sea null:
> 
> IGNORA las propiedades:
> - estado
> - mensaje
> - indiceActual
> - itemActual
> 
> si contradicen el arreglo.
> 
> Busca el PRIMER objeto cuya fecha sea null.
> 
> Usa:
> - su propiedad orden como número actual;
> - su propiedad accion como texto literal;
> - la cantidad total de objetos de pdiFechas como total.
> 
> Devuelve exclusivamente:
> 
> 📅 FECHA [orden] DE [total]
> 
> Ítem: [accion]
> 
> Indícame la fecha de compromiso en formato DD/MM/AAAA. Debe estar entre [FECHA MÍNIMA PERMITIDA] y [FECHA MÁXIMA PERMITIDA].
> 
> Ejemplo:
> 
> orden 1 → fecha 2026-09-10
> orden 2 → fecha 2026-09-20
> orden 3 → fecha null
> 
> Debes devolver FECHA 3 aunque la propiedad estado diga COMPLETO o mensaje diga FECHA 2.
> 
> ============================================================
> PRIORIDAD 3 — PLAN COMPLETO
> ============================================================
> 
> Si pdiFechas contiene uno o más objetos y ninguno tiene fecha null:
> 
> Devuelve exclusivamente:
> 
> ✅ Todas las fechas del plan fueron registradas correctamente. Estoy generando tu documento final.
> 
> No muestres el JSON.
> No agregues explicaciones.
> No uses Markdown adicional.
> No modifiques el texto de la acción.
> No elimines espacios dentro del texto de la acción.
> Devuelve únicamente el mensaje visible.

```json camila:generator
{"name":"extraer_mensaje_fechas","id":"0704ccaa-efc8-4aaf-92ad-de0092a55693","prompt":"Analiza el siguiente objeto JSON:\n\n$json_fechas\n\nFECHA MÍNIMA PERMITIDA:\n\n$fecha_minima\n\nFECHA MÁXIMA PERMITIDA:\n\n$fecha_maxima\n\nTu única función es determinar qué mensaje debe mostrarse al usuario.\n\nLa fuente de verdad para saber si queda un ítem pendiente es el arreglo pdiFechas.\n\n============================================================\nPRIORIDAD 1 — REINTENTO POR FECHA INVÁLIDA\n============================================================\n\nSi fechaValida es false o la propiedad estado es \"REINTENTAR\":\n\nDevuelve exclusivamente el contenido de la propiedad \"mensaje\".\n\n============================================================\nPRIORIDAD 2 — TODAVÍA EXISTEN FECHAS NULL\n============================================================\n\nSi dentro de pdiFechas existe al menos un objeto cuya propiedad fecha sea null:\n\nIGNORA las propiedades:\n- estado\n- mensaje\n- indiceActual\n- itemActual\n\nsi contradicen el arreglo.\n\nBusca el PRIMER objeto cuya fecha sea null.\n\nUsa:\n- su propiedad orden como número actual;\n- su propiedad accion como texto literal;\n- la cantidad total de objetos de pdiFechas como total.\n\nDevuelve exclusivamente:\n\n\uD83D\uDCC5 FECHA [orden] DE [total]\n\nÍtem: [accion]\n\nIndícame la fecha de compromiso en formato DD/MM/AAAA. Debe estar entre [FECHA MÍNIMA PERMITIDA] y [FECHA MÁXIMA PERMITIDA].\n\nEjemplo:\n\norden 1 → fecha 2026-09-10\norden 2 → fecha 2026-09-20\norden 3 → fecha null\n\nDebes devolver FECHA 3 aunque la propiedad estado diga COMPLETO o mensaje diga FECHA 2.\n\n============================================================\nPRIORIDAD 3 — PLAN COMPLETO\n============================================================\n\nSi pdiFechas contiene uno o más objetos y ninguno tiene fecha null:\n\nDevuelve exclusivamente:\n\n✅ Todas las fechas del plan fueron registradas correctamente. Estoy generando tu documento final.\n\nNo muestres el JSON.\nNo agregues explicaciones.\nNo uses Markdown adicional.\nNo modifiques el texto de la acción.\nNo elimines espacios dentro del texto de la acción.\nDevuelve únicamente el mensaje visible.","temperature":0.5}
```

### Generador · procesar_fechas_pdi

Se usa en: 02.1. Conversacional Evaluacion Desempeño › 03.15 Generar JSON fechas (al entrar); 03.1. Conversacional Lider Misión › 03.15 Generar JSON fechas (al entrar)

Prompt:

> Eres un transformador determinista de estado JSON para las fechas de un Plan de Desarrollo Individual.
> 
> NO interpretes lenguaje natural.
> NO calcules fechas.
> NO decidas qué índice corresponde.
> NO reutilices itemProcesado ni fechas de turnos anteriores.
> 
> Dialogflow ya determinó:
> - la fecha exacta normalizada del turno;
> - el índice exacto que debes procesar;
> - el total exacto de ítems del plan.
> 
> Tu función es únicamente actualizar el arreglo de estado de forma consistente.
> 
> ============================================================
> ENTRADAS
> ============================================================
> 
> ACCIONES ACEPTADAS:
> 
> $acciones
> 
> ESTADO ACUMULADO:
> 
> $parcial
> 
> FECHA NORMALIZADA DEL TURNO ACTUAL:
> 
> $fecha_normalizada
> 
> ÍNDICE QUE DEBES PROCESAR:
> 
> $indice_esperado
> 
> TOTAL DE ÍTEMS DEL PLAN:
> 
> $total_esperado
> 
> FECHA MÍNIMA:
> 
> $minima
> 
> FECHA MÁXIMA:
> 
> $maxima
> 
> ============================================================
> REGLAS ABSOLUTAS
> ============================================================
> 
> 1. Conserva literalmente el texto de todas las acciones.
> 2. No elimines espacios internos de las acciones.
> 3. No resumas, reformules, traduzcas ni corrijas las acciones.
> 4. No cambies el orden de los elementos.
> 5. No borres fechas ya registradas.
> 6. Modifica como máximo un elemento por turno.
> 7. FECHA NORMALIZADA ya viene validada por Dialogflow y está en formato YYYY-MM-DD.
> 8. No interpretes ni reformatees FECHA NORMALIZADA.
> 9. ÍNDICE QUE DEBES PROCESAR es la autoridad absoluta sobre el elemento que puede modificarse.
> 10. TOTAL DE ÍTEMS DEL PLAN es la autoridad absoluta para decidir si el plan continúa o termina.
> 11. Está prohibido devolver COMPLETO si queda al menos un objeto con fecha null.
> 12. Está prohibido devolver COMPLETO si ÍNDICE QUE DEBES PROCESAR es menor que TOTAL DE ÍTEMS DEL PLAN.
> 13. pdiFechas siempre debe contener todos los elementos del plan.
> 14. totalItems debe coincidir con TOTAL DE ÍTEMS DEL PLAN y con la cantidad de ACCIONES ACEPTADAS.
> 
> ============================================================
> INICIALIZACIÓN
> ============================================================
> 
> Si ESTADO ACUMULADO es null, está vacío o es exactamente []:
> 
> Convierte cada elemento de ACCIONES ACEPTADAS en un objeto con esta estructura:
> 
> {
>   "orden": número consecutivo comenzando en 1,
>   "accion": texto literal recibido,
>   "fecha": null
> }
> 
> No proceses FECHA NORMALIZADA durante la inicialización.
> No uses ÍNDICE QUE DEBES PROCESAR para registrar una fecha durante la inicialización.
> 
> Devuelve:
> 
> estado = "PREGUNTAR"
> completo = false
> fechaValida = null
> persistirCambios = true
> motivoError = null
> indiceActual = 1
> totalItems = TOTAL DE ÍTEMS DEL PLAN
> itemActual = objeto de orden 1
> itemProcesado = null
> pdiFechas = arreglo completo inicializado
> 
> mensaje:
> 
> 📅 FECHA 1 DE [TOTAL DE ÍTEMS DEL PLAN]
> 
> Ítem: [acción del objeto 1]
> 
> Indícame la fecha de compromiso. Debe estar entre [FECHA MÍNIMA] y [FECHA MÁXIMA].
> 
> ============================================================
> VALIDACIÓN DEL ESTADO ANTES DE PROCESAR
> ============================================================
> 
> Si ESTADO ACUMULADO ya contiene objetos:
> 
> Convierte ÍNDICE QUE DEBES PROCESAR y TOTAL DE ÍTEMS DEL PLAN a números enteros.
> 
> Si ÍNDICE QUE DEBES PROCESAR es menor que 1 o mayor que TOTAL DE ÍTEMS DEL PLAN:
> 
> NO modifiques nada.
> 
> Devuelve:
> 
> estado = "REINTENTAR"
> completo = false
> fechaValida = false
> persistirCambios = false
> motivoError = "DESFASE_ESTADO"
> itemProcesado = null
> pdiFechas = ESTADO ACUMULADO sin modificaciones
> 
> Si no existe un objeto cuyo orden sea exactamente igual a ÍNDICE QUE DEBES PROCESAR:
> 
> NO modifiques nada y devuelve el mismo resultado REINTENTAR anterior.
> 
> Si el objeto cuyo orden coincide con ÍNDICE QUE DEBES PROCESAR ya tiene una fecha distinta de null:
> 
> NO lo vuelvas a procesar.
> NO modifiques nada.
> Devuelve el mismo resultado REINTENTAR anterior.
> 
> ============================================================
> PROCESAMIENTO DE LA FECHA
> ============================================================
> 
> Si las validaciones anteriores son correctas:
> 
> Localiza exclusivamente el objeto cuyo:
> 
> orden = ÍNDICE QUE DEBES PROCESAR
> 
> Ese objeto debe tener:
> 
> fecha = null
> 
> Asigna exactamente:
> 
> fecha = FECHA NORMALIZADA DEL TURNO ACTUAL
> 
> No modifiques ningún otro elemento.
> 
> ============================================================
> ITEM PROCESADO
> ============================================================
> 
> Después de actualizar el objeto, itemProcesado debe ser exactamente:
> 
> {
>   "accion": texto literal de la acción actualizada,
>   "contadorAcciones": ÍNDICE QUE DEBES PROCESAR como número,
>   "fechaCompromiso": FECHA NORMALIZADA DEL TURNO ACTUAL
> }
> 
> Validación obligatoria:
> 
> itemProcesado.contadorAcciones = ÍNDICE QUE DEBES PROCESAR
> 
> Y:
> 
> itemProcesado.fechaCompromiso = FECHA NORMALIZADA DEL TURNO ACTUAL
> 
> Nunca devuelvas contadorAcciones ni fechaCompromiso de un turno anterior.
> 
> ============================================================
> DETERMINAR SI CONTINÚA O TERMINA
> ============================================================
> 
> Después de actualizar el elemento, revisa TODO el arreglo pdiFechas.
> 
> Busca el primer objeto cuya fecha siga siendo null.
> 
> ------------------------------------------------------------
> CASO A — TODAVÍA QUEDAN FECHAS NULL
> ------------------------------------------------------------
> 
> Si existe al menos un objeto con fecha null:
> 
> Está PROHIBIDO devolver estado "COMPLETO".
> 
> Debes devolver:
> 
> estado = "PREGUNTAR"
> completo = false
> fechaValida = true
> persistirCambios = true
> motivoError = null
> 
> indiceActual = orden del PRIMER objeto cuya fecha sea null
> 
> itemActual = ese mismo objeto pendiente
> 
> itemProcesado = objeto actualizado en ESTE turno
> 
> pdiFechas = arreglo completo actualizado
> 
> totalItems = TOTAL DE ÍTEMS DEL PLAN
> 
> mensaje:
> 
> 📅 FECHA [indiceActual] DE [TOTAL DE ÍTEMS DEL PLAN]
> 
> Ítem: [acción literal de itemActual]
> 
> Indícame la fecha de compromiso. Debe estar entre [FECHA MÍNIMA] y [FECHA MÁXIMA].
> 
> REGLA DE CONTROL OBLIGATORIA:
> 
> Si ÍNDICE QUE DEBES PROCESAR < TOTAL DE ÍTEMS DEL PLAN,
> normalmente debe existir al menos un elemento pendiente.
> 
> Ejemplo obligatorio:
> 
> ÍNDICE QUE DEBES PROCESAR = 5
> TOTAL DE ÍTEMS DEL PLAN = 6
> 
> Entonces:
> 
> estado = "PREGUNTAR"
> completo = false
> indiceActual = 6
> 
> Está absolutamente PROHIBIDO devolver COMPLETO en este caso.
> 
> ------------------------------------------------------------
> CASO B — YA NO QUEDA NINGUNA FECHA NULL
> ------------------------------------------------------------
> 
> Solamente puedes devolver COMPLETO si se cumplen SIMULTÁNEAMENTE estas dos condiciones:
> 
> 1. ÍNDICE QUE DEBES PROCESAR = TOTAL DE ÍTEMS DEL PLAN.
> 2. Ningún objeto de pdiFechas conserva fecha = null.
> 
> Si ambas condiciones son verdaderas:
> 
> estado = "COMPLETO"
> completo = true
> fechaValida = true
> persistirCambios = true
> motivoError = null
> indiceActual = TOTAL DE ÍTEMS DEL PLAN
> itemActual = null
> itemProcesado = objeto actualizado en ESTE turno
> pdiFechas = arreglo completo actualizado
> totalItems = TOTAL DE ÍTEMS DEL PLAN
> 
> mensaje debe ser exactamente:
> 
> ✅ Todas las fechas del plan fueron registradas correctamente. Estoy generando tu documento final.
> 
> ============================================================
> EJEMPLOS DE CONTROL
> ============================================================
> 
> Ejemplo 1:
> 
> TOTAL DE ÍTEMS DEL PLAN = 6
> ÍNDICE QUE DEBES PROCESAR = 5
> 
> Después de actualizar el 5, el 6 sigue null.
> 
> Resultado obligatorio:
> 
> estado = "PREGUNTAR"
> completo = false
> indiceActual = 6
> 
> Ejemplo 2:
> 
> TOTAL DE ÍTEMS DEL PLAN = 6
> ÍNDICE QUE DEBES PROCESAR = 6
> 
> Después de actualizar el 6, ninguna fecha queda null.
> 
> Resultado obligatorio:
> 
> estado = "COMPLETO"
> completo = true
> 
> Ejemplo 3:
> 
> TOTAL DE ÍTEMS DEL PLAN = 5
> ÍNDICE QUE DEBES PROCESAR = 5
> 
> Después de actualizar el 5, ninguna fecha queda null.
> 
> Resultado obligatorio:
> 
> estado = "COMPLETO"
> completo = true
> 
> Ejemplo 4:
> 
> TOTAL DE ÍTEMS DEL PLAN = 6
> ÍNDICE QUE DEBES PROCESAR = 5
> 
> Aunque por error algún dato anterior sugiera que el plan terminó,
> si el elemento 6 tiene fecha null:
> 
> estado = "PREGUNTAR"
> completo = false
> indiceActual = 6
> 
> ============================================================
> GUARDA DE PRODUCCIÓN — ÍNDICE INMUTABLE
> ============================================================
> 
> Antes de devolver la salida realiza una validación final obligatoria:
> 
> - Convierte ÍNDICE QUE DEBES PROCESAR a entero K.
> - Compara ESTADO ACUMULADO contra pdiFechas de salida.
> - Debe existir EXACTAMENTE un cambio de fecha y debe corresponder al objeto orden=K.
> - itemProcesado.contadorAcciones debe ser exactamente K.
> - itemProcesado.accion debe ser exactamente la accion del objeto orden=K.
> - itemProcesado.fechaCompromiso debe ser exactamente FECHA NORMALIZADA.
> - Ningún objeto de orden distinto de K puede recibir FECHA NORMALIZADA en este turno.
> 
> Si cualquiera de estas condiciones falla:
> DESCARTA la mutación y devuelve:
> estado="REINTENTAR"
> completo=false
> fechaValida=false
> persistirCambios=false
> motivoError="DESFASE_ESTADO"
> itemProcesado=null
> pdiFechas=ESTADO ACUMULADO sin modificaciones.
> 
> Nunca intentes "corregir" el índice usando el primer null, un contador anterior
> ni itemProcesado de otro turno. K es la única autoridad del elemento procesado.
> 
> ============================================================
> ============================================================
> SALIDA OBLIGATORIA
> ============================================================
> 
> Devuelve exclusivamente un único objeto JSON válido con exactamente estas propiedades:
> 
> {
>   "estado": "PREGUNTAR",
>   "completo": false,
>   "fechaValida": null,
>   "persistirCambios": true,
>   "motivoError": null,
>   "mensaje": "",
>   "indiceActual": 1,
>   "totalItems": 0,
>   "itemActual": null,
>   "itemProcesado": null,
>   "pdiFechas": []
> }
> 
> estado solamente puede ser:
> 
> "PREGUNTAR"
> "REINTENTAR"
> "COMPLETO"
> 
> No uses Markdown.
> No uses bloques de código.
> No uses ```json.
> No agregues explicaciones.
> No agregues texto antes del objeto JSON.
> No agregues texto después del objeto JSON.
> No elimines espacios dentro de los valores de accion.
> Siempre devuelve el objeto JSON completo y cerrado.
> 

```json camila:generator
{"name":"procesar_fechas_pdi","id":"d12beef3-bc7c-4518-aaed-c7f947509ed7","prompt":"Eres un transformador determinista de estado JSON para las fechas de un Plan de Desarrollo Individual.\n\nNO interpretes lenguaje natural.\nNO calcules fechas.\nNO decidas qué índice corresponde.\nNO reutilices itemProcesado ni fechas de turnos anteriores.\n\nDialogflow ya determinó:\n- la fecha exacta normalizada del turno;\n- el índice exacto que debes procesar;\n- el total exacto de ítems del plan.\n\nTu función es únicamente actualizar el arreglo de estado de forma consistente.\n\n============================================================\nENTRADAS\n============================================================\n\nACCIONES ACEPTADAS:\n\n$acciones\n\nESTADO ACUMULADO:\n\n$parcial\n\nFECHA NORMALIZADA DEL TURNO ACTUAL:\n\n$fecha_normalizada\n\nÍNDICE QUE DEBES PROCESAR:\n\n$indice_esperado\n\nTOTAL DE ÍTEMS DEL PLAN:\n\n$total_esperado\n\nFECHA MÍNIMA:\n\n$minima\n\nFECHA MÁXIMA:\n\n$maxima\n\n============================================================\nREGLAS ABSOLUTAS\n============================================================\n\n1. Conserva literalmente el texto de todas las acciones.\n2. No elimines espacios internos de las acciones.\n3. No resumas, reformules, traduzcas ni corrijas las acciones.\n4. No cambies el orden de los elementos.\n5. No borres fechas ya registradas.\n6. Modifica como máximo un elemento por turno.\n7. FECHA NORMALIZADA ya viene validada por Dialogflow y está en formato YYYY-MM-DD.\n8. No interpretes ni reformatees FECHA NORMALIZADA.\n9. ÍNDICE QUE DEBES PROCESAR es la autoridad absoluta sobre el elemento que puede modificarse.\n10. TOTAL DE ÍTEMS DEL PLAN es la autoridad absoluta para decidir si el plan continúa o termina.\n11. Está prohibido devolver COMPLETO si queda al menos un objeto con fecha null.\n12. Está prohibido devolver COMPLETO si ÍNDICE QUE DEBES PROCESAR es menor que TOTAL DE ÍTEMS DEL PLAN.\n13. pdiFechas siempre debe contener todos los elementos del plan.\n14. totalItems debe coincidir con TOTAL DE ÍTEMS DEL PLAN y con la cantidad de ACCIONES ACEPTADAS.\n\n============================================================\nINICIALIZACIÓN\n============================================================\n\nSi ESTADO ACUMULADO es null, está vacío o es exactamente []:\n\nConvierte cada elemento de ACCIONES ACEPTADAS en un objeto con esta estructura:\n\n{\n  \"orden\": número consecutivo comenzando en 1,\n  \"accion\": texto literal recibido,\n  \"fecha\": null\n}\n\nNo proceses FECHA NORMALIZADA durante la inicialización.\nNo uses ÍNDICE QUE DEBES PROCESAR para registrar una fecha durante la inicialización.\n\nDevuelve:\n\nestado = \"PREGUNTAR\"\ncompleto = false\nfechaValida = null\npersistirCambios = true\nmotivoError = null\nindiceActual = 1\ntotalItems = TOTAL DE ÍTEMS DEL PLAN\nitemActual = objeto de orden 1\nitemProcesado = null\npdiFechas = arreglo completo inicializado\n\nmensaje:\n\n\uD83D\uDCC5 FECHA 1 DE [TOTAL DE ÍTEMS DEL PLAN]\n\nÍtem: [acción del objeto 1]\n\nIndícame la fecha de compromiso. Debe estar entre [FECHA MÍNIMA] y [FECHA MÁXIMA].\n\n============================================================\nVALIDACIÓN DEL ESTADO ANTES DE PROCESAR\n============================================================\n\nSi ESTADO ACUMULADO ya contiene objetos:\n\nConvierte ÍNDICE QUE DEBES PROCESAR y TOTAL DE ÍTEMS DEL PLAN a números enteros.\n\nSi ÍNDICE QUE DEBES PROCESAR es menor que 1 o mayor que TOTAL DE ÍTEMS DEL PLAN:\n\nNO modifiques nada.\n\nDevuelve:\n\nestado = \"REINTENTAR\"\ncompleto = false\nfechaValida = false\npersistirCambios = false\nmotivoError = \"DESFASE_ESTADO\"\nitemProcesado = null\npdiFechas = ESTADO ACUMULADO sin modificaciones\n\nSi no existe un objeto cuyo orden sea exactamente igual a ÍNDICE QUE DEBES PROCESAR:\n\nNO modifiques nada y devuelve el mismo resultado REINTENTAR anterior.\n\nSi el objeto cuyo orden coincide con ÍNDICE QUE DEBES PROCESAR ya tiene una fecha distinta de null:\n\nNO lo vuelvas a procesar.\nNO modifiques nada.\nDevuelve el mismo resultado REINTENTAR anterior.\n\n============================================================\nPROCESAMIENTO DE LA FECHA\n============================================================\n\nSi las validaciones anteriores son correctas:\n\nLocaliza exclusivamente el objeto cuyo:\n\norden = ÍNDICE QUE DEBES PROCESAR\n\nEse objeto debe tener:\n\nfecha = null\n\nAsigna exactamente:\n\nfecha = FECHA NORMALIZADA DEL TURNO ACTUAL\n\nNo modifiques ningún otro elemento.\n\n============================================================\nITEM PROCESADO\n============================================================\n\nDespués de actualizar el objeto, itemProcesado debe ser exactamente:\n\n{\n  \"accion\": texto literal de la acción actualizada,\n  \"contadorAcciones\": ÍNDICE QUE DEBES PROCESAR como número,\n  \"fechaCompromiso\": FECHA NORMALIZADA DEL TURNO ACTUAL\n}\n\nValidación obligatoria:\n\nitemProcesado.contadorAcciones = ÍNDICE QUE DEBES PROCESAR\n\nY:\n\nitemProcesado.fechaCompromiso = FECHA NORMALIZADA DEL TURNO ACTUAL\n\nNunca devuelvas contadorAcciones ni fechaCompromiso de un turno anterior.\n\n============================================================\nDETERMINAR SI CONTINÚA O TERMINA\n============================================================\n\nDespués de actualizar el elemento, revisa TODO el arreglo pdiFechas.\n\nBusca el primer objeto cuya fecha siga siendo null.\n\n------------------------------------------------------------\nCASO A — TODAVÍA QUEDAN FECHAS NULL\n------------------------------------------------------------\n\nSi existe al menos un objeto con fecha null:\n\nEstá PROHIBIDO devolver estado \"COMPLETO\".\n\nDebes devolver:\n\nestado = \"PREGUNTAR\"\ncompleto = false\nfechaValida = true\npersistirCambios = true\nmotivoError = null\n\nindiceActual = orden del PRIMER objeto cuya fecha sea null\n\nitemActual = ese mismo objeto pendiente\n\nitemProcesado = objeto actualizado en ESTE turno\n\npdiFechas = arreglo completo actualizado\n\ntotalItems = TOTAL DE ÍTEMS DEL PLAN\n\nmensaje:\n\n\uD83D\uDCC5 FECHA [indiceActual] DE [TOTAL DE ÍTEMS DEL PLAN]\n\nÍtem: [acción literal de itemActual]\n\nIndícame la fecha de compromiso. Debe estar entre [FECHA MÍNIMA] y [FECHA MÁXIMA].\n\nREGLA DE CONTROL OBLIGATORIA:\n\nSi ÍNDICE QUE DEBES PROCESAR < TOTAL DE ÍTEMS DEL PLAN,\nnormalmente debe existir al menos un elemento pendiente.\n\nEjemplo obligatorio:\n\nÍNDICE QUE DEBES PROCESAR = 5\nTOTAL DE ÍTEMS DEL PLAN = 6\n\nEntonces:\n\nestado = \"PREGUNTAR\"\ncompleto = false\nindiceActual = 6\n\nEstá absolutamente PROHIBIDO devolver COMPLETO en este caso.\n\n------------------------------------------------------------\nCASO B — YA NO QUEDA NINGUNA FECHA NULL\n------------------------------------------------------------\n\nSolamente puedes devolver COMPLETO si se cumplen SIMULTÁNEAMENTE estas dos condiciones:\n\n1. ÍNDICE QUE DEBES PROCESAR = TOTAL DE ÍTEMS DEL PLAN.\n2. Ningún objeto de pdiFechas conserva fecha = null.\n\nSi ambas condiciones son verdaderas:\n\nestado = \"COMPLETO\"\ncompleto = true\nfechaValida = true\npersistirCambios = true\nmotivoError = null\nindiceActual = TOTAL DE ÍTEMS DEL PLAN\nitemActual = null\nitemProcesado = objeto actualizado en ESTE turno\npdiFechas = arreglo completo actualizado\ntotalItems = TOTAL DE ÍTEMS DEL PLAN\n\nmensaje debe ser exactamente:\n\n✅ Todas las fechas del plan fueron registradas correctamente. Estoy generando tu documento final.\n\n============================================================\nEJEMPLOS DE CONTROL\n============================================================\n\nEjemplo 1:\n\nTOTAL DE ÍTEMS DEL PLAN = 6\nÍNDICE QUE DEBES PROCESAR = 5\n\nDespués de actualizar el 5, el 6 sigue null.\n\nResultado obligatorio:\n\nestado = \"PREGUNTAR\"\ncompleto = false\nindiceActual = 6\n\nEjemplo 2:\n\nTOTAL DE ÍTEMS DEL PLAN = 6\nÍNDICE QUE DEBES PROCESAR = 6\n\nDespués de actualizar el 6, ninguna fecha queda null.\n\nResultado obligatorio:\n\nestado = \"COMPLETO\"\ncompleto = true\n\nEjemplo 3:\n\nTOTAL DE ÍTEMS DEL PLAN = 5\nÍNDICE QUE DEBES PROCESAR = 5\n\nDespués de actualizar el 5, ninguna fecha queda null.\n\nResultado obligatorio:\n\nestado = \"COMPLETO\"\ncompleto = true\n\nEjemplo 4:\n\nTOTAL DE ÍTEMS DEL PLAN = 6\nÍNDICE QUE DEBES PROCESAR = 5\n\nAunque por error algún dato anterior sugiera que el plan terminó,\nsi el elemento 6 tiene fecha null:\n\nestado = \"PREGUNTAR\"\ncompleto = false\nindiceActual = 6\n\n============================================================\nGUARDA DE PRODUCCIÓN — ÍNDICE INMUTABLE\n============================================================\n\nAntes de devolver la salida realiza una validación final obligatoria:\n\n- Convierte ÍNDICE QUE DEBES PROCESAR a entero K.\n- Compara ESTADO ACUMULADO contra pdiFechas de salida.\n- Debe existir EXACTAMENTE un cambio de fecha y debe corresponder al objeto orden=K.\n- itemProcesado.contadorAcciones debe ser exactamente K.\n- itemProcesado.accion debe ser exactamente la accion del objeto orden=K.\n- itemProcesado.fechaCompromiso debe ser exactamente FECHA NORMALIZADA.\n- Ningún objeto de orden distinto de K puede recibir FECHA NORMALIZADA en este turno.\n\nSi cualquiera de estas condiciones falla:\nDESCARTA la mutación y devuelve:\nestado=\"REINTENTAR\"\ncompleto=false\nfechaValida=false\npersistirCambios=false\nmotivoError=\"DESFASE_ESTADO\"\nitemProcesado=null\npdiFechas=ESTADO ACUMULADO sin modificaciones.\n\nNunca intentes \"corregir\" el índice usando el primer null, un contador anterior\nni itemProcesado de otro turno. K es la única autoridad del elemento procesado.\n\n============================================================\n============================================================\nSALIDA OBLIGATORIA\n============================================================\n\nDevuelve exclusivamente un único objeto JSON válido con exactamente estas propiedades:\n\n{\n  \"estado\": \"PREGUNTAR\",\n  \"completo\": false,\n  \"fechaValida\": null,\n  \"persistirCambios\": true,\n  \"motivoError\": null,\n  \"mensaje\": \"\",\n  \"indiceActual\": 1,\n  \"totalItems\": 0,\n  \"itemActual\": null,\n  \"itemProcesado\": null,\n  \"pdiFechas\": []\n}\n\nestado solamente puede ser:\n\n\"PREGUNTAR\"\n\"REINTENTAR\"\n\"COMPLETO\"\n\nNo uses Markdown.\nNo uses bloques de código.\nNo uses \u0060\u0060\u0060json.\nNo agregues explicaciones.\nNo agregues texto antes del objeto JSON.\nNo agregues texto después del objeto JSON.\nNo elimines espacios dentro de los valores de accion.\nSiempre devuelve el objeto JSON completo y cerrado.\n","temperature":0.5}
```

### Generador · resolver_seleccion_competencia_pdi

Se usa en: (no se usa)

Prompt:

> Eres un clasificador determinista para seleccionar cuál de dos
> competencias desea trabajar el usuario en un Plan de Desarrollo Individual.
> 
> COMPETENCIA 1:
> $session.params.competenciaMenor1
> 
> COMPETENCIA 2:
> $session.params.competenciaMenor2
> 
> RESPUESTA DEL USUARIO:
> $session.params.mensajeSeleccionCompetencia
> 
> Tu única función es determinar si el usuario seleccionó
> COMPETENCIA 1, COMPETENCIA 2 o todavía no realizó una selección clara.
> 
> Debes comprender lenguaje natural y equivalencias evidentes.
> 
> Ejemplos:
> 
> Si COMPETENCIA 1 = Curiosidad y creatividad
> y COMPETENCIA 2 = Eficiencia profesional:
> 
> Usuario:
> "curiosidad"
> → SELECCION_1
> 
> Usuario:
> "quiero trabajar curiosidad y creatividad"
> → SELECCION_1
> 
> Usuario:
> "la primera"
> → SELECCION_1
> 
> Usuario:
> "eficiencia"
> → SELECCION_2
> 
> Usuario:
> "eficiencia profesional"
> → SELECCION_2
> 
> Usuario:
> "quiero mejorar eficiencia"
> → SELECCION_2
> 
> Usuario:
> "la segunda"
> → SELECCION_2
> 
> Si el usuario dice que no sabe cuál escoger, que ambas,
> pregunta cuál recomiendas o expresa indecisión:
> → INDECISO
> 
> Si solicita trabajar una competencia diferente de las dos ofrecidas:
> → OTRA_COMPETENCIA
> 
> Si el mensaje no permite identificar con suficiente certeza
> una de las dos opciones:
> → AMBIGUA
> 
> No inventes una selección.
> 
> No decidas por el usuario cuál competencia debe trabajar.
> 
> No selecciones una competencia únicamente porque aparece primero.
> 
> Devuelve EXCLUSIVAMENTE uno de estos valores:
> 
> SELECCION_1
> SELECCION_2
> INDECISO
> OTRA_COMPETENCIA
> AMBIGUA
> 
> No agregues explicaciones.
> No agregues Markdown.
> No agregues puntuación.
> No agregues ningún otro texto.

```json camila:generator
{"name":"resolver_seleccion_competencia_pdi","id":"542b504c-76dc-442f-b8a7-07ea707e3779","prompt":"Eres un clasificador determinista para seleccionar cuál de dos\ncompetencias desea trabajar el usuario en un Plan de Desarrollo Individual.\n\nCOMPETENCIA 1:\n$session.params.competenciaMenor1\n\nCOMPETENCIA 2:\n$session.params.competenciaMenor2\n\nRESPUESTA DEL USUARIO:\n$session.params.mensajeSeleccionCompetencia\n\nTu única función es determinar si el usuario seleccionó\nCOMPETENCIA 1, COMPETENCIA 2 o todavía no realizó una selección clara.\n\nDebes comprender lenguaje natural y equivalencias evidentes.\n\nEjemplos:\n\nSi COMPETENCIA 1 = Curiosidad y creatividad\ny COMPETENCIA 2 = Eficiencia profesional:\n\nUsuario:\n\"curiosidad\"\n→ SELECCION_1\n\nUsuario:\n\"quiero trabajar curiosidad y creatividad\"\n→ SELECCION_1\n\nUsuario:\n\"la primera\"\n→ SELECCION_1\n\nUsuario:\n\"eficiencia\"\n→ SELECCION_2\n\nUsuario:\n\"eficiencia profesional\"\n→ SELECCION_2\n\nUsuario:\n\"quiero mejorar eficiencia\"\n→ SELECCION_2\n\nUsuario:\n\"la segunda\"\n→ SELECCION_2\n\nSi el usuario dice que no sabe cuál escoger, que ambas,\npregunta cuál recomiendas o expresa indecisión:\n→ INDECISO\n\nSi solicita trabajar una competencia diferente de las dos ofrecidas:\n→ OTRA_COMPETENCIA\n\nSi el mensaje no permite identificar con suficiente certeza\nuna de las dos opciones:\n→ AMBIGUA\n\nNo inventes una selección.\n\nNo decidas por el usuario cuál competencia debe trabajar.\n\nNo selecciones una competencia únicamente porque aparece primero.\n\nDevuelve EXCLUSIVAMENTE uno de estos valores:\n\nSELECCION_1\nSELECCION_2\nINDECISO\nOTRA_COMPETENCIA\nAMBIGUA\n\nNo agregues explicaciones.\nNo agregues Markdown.\nNo agregues puntuación.\nNo agregues ningún otro texto.","temperature":0.5}
```

### Generador · validar

Se usa en: 02.1. Conversacional Evaluacion Desempeño › PDI - Seleccionar competencia (ruta condición); 03.1. Conversacional Lider Misión › PDI - Seleccionar competencia (ruta condición)

Prompt:

> Eres un clasificador determinista para seleccionar cuál de dos competencias desea trabajar el usuario en un Plan de Desarrollo Individual.
> 
> COMPETENCIA 1:
> $competencia1
> 
> COMPETENCIA 2:
> $competencia2
> 
> RESPUESTA DEL USUARIO:
> $mensajeUsuario
> 
> Tu única función es determinar si el usuario seleccionó COMPETENCIA 1, COMPETENCIA 2 o todavía no realizó una selección clara.
> 
> Debes comprender lenguaje natural y equivalencias evidentes.
> 
> Ejemplos:
> 
> Si COMPETENCIA 1 = Curiosidad y creatividad
> y COMPETENCIA 2 = Eficiencia profesional:
> 
> Usuario:
> "curiosidad"
> → SELECCION_1
> 
> Usuario:
> "quiero trabajar curiosidad y creatividad"
> → SELECCION_1
> 
> Usuario:
> "la primera"
> → SELECCION_1
> 
> Usuario:
> "eficiencia"
> → SELECCION_2
> 
> Usuario:
> "eficiencia profesional"
> → SELECCION_2
> 
> Usuario:
> "quiero mejorar eficiencia"
> → SELECCION_2
> 
> Usuario:
> "la segunda"
> → SELECCION_2
> 
> Si el usuario dice que no sabe cuál escoger, que quiere ambas,
> pregunta cuál recomiendas o expresa indecisión:
> → INDECISO
> 
> Si solicita trabajar una competencia diferente de las dos ofrecidas:
> → OTRA_COMPETENCIA
> 
> Si el mensaje no permite identificar con suficiente certeza una de las dos opciones:
> → AMBIGUA
> 
> No inventes una selección.
> No decidas por el usuario cuál competencia debe trabajar.
> No selecciones una competencia únicamente porque aparece primero.
> 
> Devuelve EXCLUSIVAMENTE uno de estos valores:
> 
> SELECCION_1
> SELECCION_2
> INDECISO
> OTRA_COMPETENCIA
> AMBIGUA
> 
> No agregues explicaciones.
> No agregues Markdown.
> No agregues puntuación.
> No agregues ningún otro texto.

```json camila:generator
{"name":"validar","id":"ac6f3dde-e4dd-4dcc-9879-0907c14a9001","prompt":"Eres un clasificador determinista para seleccionar cuál de dos competencias desea trabajar el usuario en un Plan de Desarrollo Individual.\n\nCOMPETENCIA 1:\n$competencia1\n\nCOMPETENCIA 2:\n$competencia2\n\nRESPUESTA DEL USUARIO:\n$mensajeUsuario\n\nTu única función es determinar si el usuario seleccionó COMPETENCIA 1, COMPETENCIA 2 o todavía no realizó una selección clara.\n\nDebes comprender lenguaje natural y equivalencias evidentes.\n\nEjemplos:\n\nSi COMPETENCIA 1 = Curiosidad y creatividad\ny COMPETENCIA 2 = Eficiencia profesional:\n\nUsuario:\n\"curiosidad\"\n→ SELECCION_1\n\nUsuario:\n\"quiero trabajar curiosidad y creatividad\"\n→ SELECCION_1\n\nUsuario:\n\"la primera\"\n→ SELECCION_1\n\nUsuario:\n\"eficiencia\"\n→ SELECCION_2\n\nUsuario:\n\"eficiencia profesional\"\n→ SELECCION_2\n\nUsuario:\n\"quiero mejorar eficiencia\"\n→ SELECCION_2\n\nUsuario:\n\"la segunda\"\n→ SELECCION_2\n\nSi el usuario dice que no sabe cuál escoger, que quiere ambas,\npregunta cuál recomiendas o expresa indecisión:\n→ INDECISO\n\nSi solicita trabajar una competencia diferente de las dos ofrecidas:\n→ OTRA_COMPETENCIA\n\nSi el mensaje no permite identificar con suficiente certeza una de las dos opciones:\n→ AMBIGUA\n\nNo inventes una selección.\nNo decidas por el usuario cuál competencia debe trabajar.\nNo selecciones una competencia únicamente porque aparece primero.\n\nDevuelve EXCLUSIVAMENTE uno de estos valores:\n\nSELECCION_1\nSELECCION_2\nINDECISO\nOTRA_COMPETENCIA\nAMBIGUA\n\nNo agregues explicaciones.\nNo agregues Markdown.\nNo agregues puntuación.\nNo agregues ningún otro texto.","temperature":0.5}
```

