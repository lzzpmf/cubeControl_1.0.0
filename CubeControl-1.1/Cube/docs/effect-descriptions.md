# Descrições breves dos efeitos

[Documentação](README.md)

Pesquisa histórica em 18/09/2026, aplicada na versão 3.3.8. São explicações gerais dos efeitos, não especificações dos circuitos fotografados nem afirmações sobre o algoritmo interno do Cube Baby. Textos em português e inglês disponíveis na interface.

| Efeito | Texto abaixo do título | Fonte |
|---|---|---|
| Chorus | Mistura o som original com uma cópia levemente desafinada, criando corpo e movimento. | [Native Instruments — Chorus effect](https://blog.native-instruments.com/chorus-effect/) |
| Phaser | Cria uma varredura de fase que dá ao timbre um movimento ondulante. | [BOSS — Five Creative Ways to Use a Phaser Pedal](https://articles.boss.info/five-creative-ways-to-use-a-phaser-pedal-for-guitar/) |
| Delay | Repete o som após um intervalo, criando ecos e padrões rítmicos. | [BOSS — The Complete Guide to Delay Pedals](https://articles.boss.info/the-complete-guide-to-delay-pedals/) |
| Reverb | Simula reflexões sonoras de um ambiente, acrescentando espaço e profundidade. | [Waves — Get More From Reverb](https://www.waves.com/get-more-from-reverb) |

Implementação atual: [GearItem.Subtitle](../windows/Cube.Desktop/ViewModels/VisualCatalogViewModel.cs), com traduções em [Localization](../windows/Cube.Desktop/Localization/README.md). As fontes externas registram a origem da pesquisa; não certificam o algoritmo do pedal.

As categorias Modulação, Repetições e Ambiência não aparecem no canto superior direito desses quatro painéis. O nome superior esquerdo é preservado. A descrição substitui a categoria abaixo do título, com quebra automática de linha. Os slots dos cabinets e as categorias dos amplificadores permanecem como estavam.
