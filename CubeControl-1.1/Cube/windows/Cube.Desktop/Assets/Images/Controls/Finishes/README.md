# Acabamentos dos controles

Assets criados com a ferramenta integrada `image_gen`, com fundo transparente.
Cada conjunto contém seis PNGs, extraídos das células de uma folha 1536 × 1024,
com três colunas e duas linhas. Os originais oficiais permanecem preservados.
O serviço recorta margens transparentes e armazena as imagens em cache.

## Materiais usados nos prompts

- `official-light`: light neutral satin silver faces, graphite indicators, polished silver rims
- `brushed-silver`: brushed aluminum silver faces with fine parallel metal grain, dark graphite indicator lines, chrome rims
- `retro-ivory`: warm ivory cream faces, aged nickel rims, deep brown indicator lines
- `graphite-copper`: charcoal graphite faces, brushed copper rims, ivory indicator lines

## Prompt base

Substituir `{material}` por um dos materiais acima. O mesmo prompt foi usado
para os quatro conjuntos:

```text
Create one production UI sprite sheet PNG on truly transparent background for CubeControl musical controls. EXACT layout: 1536x1024 canvas, 3 equal columns and 2 equal rows, 512x512 cells. Each cell contains exactly one isolated centered object, no labels, no text, no numbers, no shadows outside object, no perspective. Material for all six: {material}. Top-left: circular knob front orthographic view, thin metallic rim, broad flat subtly convex face, ONE short indicator at 12 o'clock, diameter 380px. Top-middle: vertical-slider cap (wide horizontal rectangle), beveled 3D cap 360x160px with centered horizontal indicator stripe, no track. Top-right: horizontal-slider cap (tall narrow rectangle), beveled 160x340px with centered vertical indicator stripe, no track. Bottom-left: horizontal toggle switch OFF, rounded capsule 400x190px, knob at LEFT, dim indicator on RIGHT. Bottom-middle: same exact horizontal toggle switch ON, knob at RIGHT, illuminated warm indicator on LEFT. Bottom-right: round metal footswitch front/top orthographic view, concentric polished metal rings and raised central pushbutton, 360px diameter, no indicator. Consistent premium realistic physical audio-equipment rendering, subtle bevels, lighting upper left, designed to read at 30-90px sizes. Preserve clean alpha outside each object. No extra objects.
```

As capturas de validação são geradas pelo modo `--theme-only` em
`C:/CubeControl-Build/reports/control-finishes`.

## Player

Cada acabamento também contém `player-face.png`: uma face circular sem marcador,
gerada com `image_gen`. Os 12 símbolos são desenhados pelo controle WPF sobre
essa face, preservando a nitidez em diferentes escalas. Transporte e som/mudo usam
geometrias vetoriais. O acabamento Oficial no escuro conserva os PNGs originais do player.

Prompt utilizado para cada face, substituindo `{material}`:

```text
Production UI bitmap asset for CubeControl audio player. A single circular physical audio-equipment button cap, front orthographic top view, perfectly round, centered in square canvas with equal small transparent margins, diameter 85% of canvas. {material}. Broad subtly convex flat face, thin metallic edge matching premium amplifier knobs, soft restrained highlights, subtle realistic texture, readable at 48px. Absolutely blank center and blank whole face: NO indicator, NO line, NO symbol, NO lettering, NO holes, NO icon, NO pointer. Lighting upper left. Truly transparent background outside the circular button, NO cast shadow outside object, NO other objects. Output 512x512 square PNG.
```

Materiais usados: `neutral light satin silver face, thin polished chrome rim`;
`brushed aluminum silver face, fine parallel metal grain, polished chrome rim`;
`warm ivory cream face, aged nickel metallic rim`;
`dark charcoal graphite face, brushed copper rim`.

## Sete conjuntos do acervo

| Acabamento | Pasta | Knob original | Fader original |
| --- | --- | --- | --- |
| Preto Marfim | `black-ivory` | Preto com indicador marfim | Deslizante retrô preto com faixa creme |
| Preto Industrial | `industrial-black` | Industrial preto com centro metálico | Deslizante de áudio preto com faixa central |
| Aço Texturizado | `textured-steel` | Metálico de controle texturizado | Preto com indicador prateado |
| Preto Marconi | `marconi-black` | Rotativo vintage preto Marconi | Retrô charcoal com faixa creme |
| Latão Vintage | `vintage-brass` | Vintage de metal dourado | Vintage de estilo retrô |
| Marfim Dourado | `ivory-gold` | Marfim com ponteiro dourado | Criados nas duas orientações |
| Crânio Azul | `chrome-skull-blue` | Crânio cromado polido | Futurista com faixa azul luminosa |

As sete pastas contêm 49 PNGs: 13 cópias idênticas dos originais e 36 complementos
gerados com `image_gen`. As imagens de arquivo permanecem intocadas. O fader
original Preto Marfim é usado na orientação vertical; os demais originais são
usados na horizontal. A orientação complementar é gerada na mesma família.

[acervo-manifest.json](acervo-manifest.json) registra os caminhos exatos dos
originais, as referências de geração, os prompts completos e os sprites de origem.
Cada sprite possui três colunas; a separação de recorte entre as duas linhas foi
ajustada para 480 pixels em uma folha de 1536 × 1024, preservando as bordas das peças.

O crânio mantém sua proporção e gira com o valor, com marcador externo sincronizado.
O knob Preto Marfim usa pivô vertical de 61,5% da imagem recortada para alinhar
a rotação ao eixo físico da peça. Os demais usam o centro da imagem.
