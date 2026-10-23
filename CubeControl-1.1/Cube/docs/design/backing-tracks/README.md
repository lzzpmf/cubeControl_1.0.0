# Backing Tracks — proposta B implementada

Em 22/10/2026, o usuário escolheu a proposta B: preparação da guitarra e revisão de tomadas à esquerda. A implementação segue essa organização em uma View WPF dedicada.

- [Relatório de implementação e validação](validacao.md).
- [Tela anterior](estado-atual.png), capturada no WPF com dados isolados.
- Conceitos: [A](proposta-a.png), [B escolhida](proposta-b.png), [gravação](estado-gravando.png), [edição](estado-edicao.png) e [tema claro](estado-claro.png).
- Prompts completos: [A/B](concept-prompts.json) e [estados](state-prompts.json).
- Capturas da implementação: [1920×1080](implementacao-1920.png), [640×480](implementacao-640.png), [painéis abertos](implementacao-paineis.png) e [tema claro](implementacao-claro.png).

Os conceitos são referências visuais, não evidências funcionais. Possuem 1672×941 pixels. Crossfade é um comando condicionado à seleção, embora alguns conceitos possam sugerir uma preferência liga/desliga.

As capturas reais usam RenderTargetBitmap, áudio sintético e diretórios isolados, sem reprodução, captura do microfone ou comunicação com o pedal. Transparência/Mica pode aparecer preta nesse método. As imagens não comprovam operação em diferentes monitores, acessibilidade por Narrator ou desempenho de áudio ao vivo.
