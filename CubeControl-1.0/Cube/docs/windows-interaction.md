# Interação, zoom e aparência do Windows

- Zoom de 50% a 200%, em passos de 10%, nas reticências. Menos, percentual, mais e restaurar. Atalhos: Ctrl +, Ctrl -, Ctrl 0. Preferência local em `%LOCALAPPDATA%/CubeControl/zoom-v1.json`; 100% preserva a densidade anterior de 90%. Não modifica as configurações de DPI do Windows. O manifesto declara PerMonitorV2.
- Rolagem da roda nas páginas e diálogos interpolada por 180 ms, com alvo acumulado para entrada rápida/fracionada. `CompositionTarget.Rendering` agenda os quadros, sem timer de 16 ms ou loop permanente. A inscrição termina ao concluir, ocultar ou descarregar a superfície; navegação pelo teclado/toque e reposicionamento externo interrompem o movimento. Limites permitem continuar a rolagem no contêiner externo.
- Knobs, seleção de equipamento e cadeia preservam suas interações da roda. Zoom, layout e navegação não enviam comandos MIDI.
- Transições de opacidade e deslocamento usam 167 ms, com `DesiredFrameRate=240`. Animações desativadas no Windows, alto contraste ou renderização sem aceleração desativam os movimentos decorativos. A duração da rolagem depende do tempo transcorrido, não da contagem de quadros.
- Mica acompanha `EnableTransparency` e notificações do Windows mesmo com tema Claro/Escuro manual. Transparência desligada, sistema incompatível ou alto contraste usam fundo sólido. A janela mantém o chrome existente para evitar o erro de contexto Freezable ao trocar WindowChrome no WPF UI 4.3.
- A/B/C e ações se reorganizam em larguras pequenas. Diálogos com pouco espaço reduzem margens e quebram texto dos botões, preservando o conteúdo rolável e os comandos nativos.

## Verificação

Testes com MIDI simulado: `--zoom-only` (50/100/150/200%, pt-BR/en-US, 640–1920 DIPs), `--zoom-dialogs-only`, `--options-menu-only`, `--motion-only`, `--theme-only`, `--wheel-only`, `--localization-only`. O teste dos diálogos exige viewport útil e ações dentro do host, além de capturas para revisão visual.

`--scroll-profile` registra intervalos entre callbacks WPF no editor conectado ao simulador em `windows/artifacts/interaction/scroll-profile.json`. Uma execução local a 1280×720, render tier 2, registrou 289 intervalos: mediana de 4 ms e percentil 95 de 12 ms. Esses valores são callbacks de renderização; não são FPS efetivamente apresentados pelo DWM/monitor. Não certificam 240 FPS constantes, ausência universal de engasgos, nem troca física entre monitores com DPIs diferentes.

## Referências

- [Microsoft: renderização por quadro no WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/how-to-render-on-a-per-frame-interval-using-compositiontarget)
- [Microsoft: DesiredFrameRate é uma meta, não uma garantia](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.animation.timeline.desiredframerate?view=windowsdesktop-10.0)
- [Microsoft: Mica e fallback de transparência](https://learn.microsoft.com/en-us/windows/apps/design/style/mica)
