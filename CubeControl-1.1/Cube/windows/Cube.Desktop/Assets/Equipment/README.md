# Imagens de equipamentos e controles

[Documentação](../../../../docs/README.md) · Revisão: 01/10/2026

As fotos padrão têm origem nos arquivos fornecidos pelo usuário durante o desenvolvimento. Esta revisão conferiu os arquivos presentes e seu mapeamento; os originais anteriores à movimentação não estão disponíveis para repetir a comparação de bytes. O projeto copia os PNGs para `Assets/Equipment` ao compilar/publicar.

## Identidades de equipamentos

| Arquivo | Identidade estável |
| --- | --- |
| chorusCuvave.png | chorus |
| phaserCuvave.png | phaser |
| delayCuvave.png | delay |
| reverbCuvave.png | reverb |
| Koch Powertone.png | amp-0 |
| Friedman BE-100.png | amp-1 |
| Two-Rock Coral.png | amp-2 |
| Dr. Z MAZ 38.png | amp-3 |
| MesaBoogie JP-2C.png | amp-4 |
| Vox AC30.png | amp-5 |
| VHT Pittbull.png | amp-6 |
| Two Rock Coral- variante.png | amp-7 |
| PRS Archon.png | amp-8 |
| Line 6 Vetta.png | factory-cab-0 |
| Marshall 1960AV.png | factory-cab-1 |
| Marshall 1960A 4×12 com G12T-75.png | factory-cab-2 |
| DirectBox.png | cab-off (imagem da opção Sem cabinet; não simula uma DI) |
| VHT Deliverance.png | factory-cab-3 |
| Soldano.png | factory-cab-4 |
| Peavey 5150 + Mesa Oversized.png | factory-cab-5 |
| JSX KT77 + Mesa Stiletto AKG C1000.png | factory-cab-6 |
| Diezel V30 SM57 (Soldano HR PowerAmp).png | factory-cab-7 |

As identidades de fábrica dos IRs vêm do SHA-256 do conteúdo reconhecido, não da posição atual. Fotos personalizadas ficam em `Images` na pasta de dados do editor (`%LOCALAPPDATA%\CubeControl` ou `CubeControl-Simulation`) e têm prioridade. Não atribuir fotos de fábrica a IRs desconhecidos. Referências comerciais das fotos não comprovam a identidade sonora de um amp; veja o [catálogo](../../../../docs/ampsEirs.md).

## Outros recursos presentes

| Arquivo / grupo | Uso verificado |
| --- | --- |
| `footswitch.png` | Fundo dos seletores A/B/C na janela |
| `gabineteGenerico.png`, `micGenerico.png` | Cena de distância do microfone |
| `cuvaveBaby.png` | Recurso disponível; sem referência direta localizada no C#/XAML atual |
| Quatro PNGs iniciados por `Botão` | Nomes referenciados por `AmpControl.Asset` |
| Quatro PNGs iniciados por `Fader` | Nomes referenciados por `AmpControl.Asset` |

O XAML atual do criador usa o controle desenhado `Knob`/fader e não faz binding de `AmpControl.Asset`; a presença das texturas não comprova sua exibição. Não confundir acervo disponível com recurso efetivamente renderizado.

Fontes: [VisualCatalogViewModel.cs](../../ViewModels/VisualCatalogViewModel.cs), [AmpViewModel.cs](../../ViewModels/AmpViewModel.cs), [AmpEditor.xaml](../../Views/AmpEditor.xaml), [MicrophoneDistance.cs](../../Controls/MicrophoneDistance.cs) e [projeto](../../Cube.Desktop.csproj).
