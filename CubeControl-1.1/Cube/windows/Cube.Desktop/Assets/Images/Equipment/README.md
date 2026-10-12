# Imagens de equipamentos e controles

[Documentação](../../../../../docs/README.md) · Revisão de mapeamentos: 12/10/2026

As fotos padrão têm origem nos arquivos fornecidos pelo usuário durante o desenvolvimento. Esta revisão conferiu os arquivos presentes e seu mapeamento; os originais anteriores à movimentação não estão disponíveis para repetir a comparação de bytes. O projeto copia PNG/JPEG de equipamentos para `Assets/Images/Equipment`. Artes oficiais de controles são recursos WPF embutidos; imagens auxiliares selecionadas são copiadas como arquivos externos, conforme o `.csproj`.

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
| Line 6 Vetta.png | pack-bfi-cab-0 |
| Marshall 1960AV.png | pack-bfi-cab-1 |
| Marshall 1960A 4×12 com G12T-75.png | pack-bfi-cab-2 |
| DirectBox.png | cab-off (imagem da opção Sem cabinet; não simula uma DI) |
| VHT Deliverance.png | pack-bfi-cab-3 |
| Soldano.png | pack-bfi-cab-4 |
| Peavey 5150 + Mesa Oversized.png | pack-bfi-cab-5 |
| JSX KT77 + Mesa Stiletto AKG C1000.png | pack-bfi-cab-6 |
| Diezel V30 SM57 (Soldano HR PowerAmp).png | pack-bfi-cab-7 |

As identidades de fábrica dos IRs vêm do SHA-256 do conteúdo reconhecido, não da posição atual. Fotos personalizadas ficam em `Images` na pasta de dados do editor (`%LOCALAPPDATA%\CubeControl` ou `CubeControl-Simulation`) e têm prioridade. Não atribuir fotos de fábrica a IRs desconhecidos. Referências comerciais das fotos não comprovam a identidade sonora de um amp; veja o [catálogo](../../../../../docs/referencias/ampsEirs.md).

## Outros recursos presentes

| Arquivo / grupo | Uso verificado |
| --- | --- |
| `footswitch_oficial_01.png` | Arte oficial embutida de footswitch; `ControlArtwork` resolve o acabamento selecionado e o fallback de alto contraste |
| `gabineteGenerico.png`, `micGenerico.png` | Cena de distância do microfone |
| `cuvaveBaby.png` | Recurso disponível; sem referência direta localizada no C#/XAML atual |
| Quatro PNGs iniciados por `Botão` | Nomes referenciados por `AmpControl.Asset` |
| Quatro PNGs iniciados por `Fader` | Nomes referenciados por `AmpControl.Asset` |

O XAML atual do criador usa o controle desenhado `Knob`/fader e não faz binding de `AmpControl.Asset`; a presença das texturas não comprova sua exibição. Não confundir acervo disponível com recurso efetivamente renderizado.

Fontes: [VisualCatalogViewModel.cs](../../../ViewModels/VisualCatalogViewModel.cs), [AmpViewModel.cs](../../../ViewModels/AmpViewModel.cs), [AmpEditor.xaml](../../../Views/AmpEditor.xaml), [MicrophoneDistance.cs](../../../Controls/MicrophoneDistance.cs) e [projeto](../../../Cube.Desktop.csproj).

## Artes ilustrativas dos IRs — 08/10/2026

As novas artes foram geradas com ImageGen usando as imagens de `Assets/Archive/Images/Cabeçotes` como referência: vista frontal simétrica, ilustração detalhada, textura de revestimento e tecido, iluminação suave, sem logotipos e PNG com transparência real. São representações visuais inferidas dos nomes; não comprovam gabinete, falante ou identidade acústica da captura. A identificação de conteúdo continua independente em `CabinetInventory`.

`CabinetArtwork` associa caminho/nome à família visual. Variações de microfone compartilham a arte. A identidade de fábrica reconhecida tem precedência; imagens personalizadas continuam prioritárias na Pedaleira.

| Nome/família do IR | Arte |
| --- | --- |
| Fender Deluxe Reverb Oxford | `fender-deluxe-oxford-1x12.png` |
| Fender Twin Reverb | `fender-twin-reverb-2x12.png` |
| Marshall 1960 / 1985 Custom | `marshall-1960-custom-4x12.png` |
| Marshall 1960AV | `Marshall 1960AV.png` (arte existente reaproveitada) |
| Marshall 1960TV Greenback M25 | `marshall-1960tv-greenback-4x12.png` |
| Mesa 4FB Vintage 30 | `mesa-4fb-v30-4x12.png` |
| Mesa Rectifier Vintage 30 | `mesa-rectifier-v30.png` (dimensões da captura não confirmadas) |
| Mesa Oversized | `Peavey 5150 + Mesa Oversized.png` (arte existente reaproveitada) |
| Orange 2x12 Vintage 30 | `orange-v30-2x12.png` |
| Vox AC30 Blue | `vox-ac30-blue-2x12.png` |
| 2x12 vertical / upright explícito no nome | `cabinet-vertical-2x12.png` |
| Orange 2x12 vertical explícito no nome | `orange-v30-vertical-2x12.png` |
| Sem família reconhecida / BFIN | `cabinet-generic-front.png` |

A arte genérica frontal substitui a vista em três quartos nos cartões e na galeria. `gabineteGenerico.png` permanece na cena funcional do microfone, cuja perspectiva mostra a distância. As oito artes específicas de fábrica foram inspecionadas e reaproveitadas por já seguirem a composição frontal. Arquivos originais do acervo são preservados.

As duas artes de 2x12 vertical foram refeitas a partir da foto fornecida pelo usuário: gabinete compacto com proporção frontal de aproximadamente 0,75 entre largura e altura, moldura reta, grade escura densa e falantes discretos sob o tecido. As versões anteriores estreitas, com cones evidentes, foram substituídas nos mesmos arquivos.

As artes horizontais de Fender Twin Reverb, Vox AC30 Blue e Orange Vintage 30 foram refeitas com a proporção da segunda foto de referência: largura próxima de 1,4 vez a altura, grade alta e gabinete compacto. As versões achatadas foram substituídas nos mesmos arquivos, preservando os materiais e as cores de cada família.


## Asset organization - 08/10/2026

All moved images were verified using SHA-256. See [movement manifest](../../asset-moves.json) and [asset structure](../../README.md). Equipment identities are preserved; unused alternatives are archived.
