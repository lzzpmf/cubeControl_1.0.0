# Catálogo de amps e gabinetes

[Documentação](../README.md) · Revisão: 01/10/2026

Esta página separa **índices do protocolo**, **aliases do pedal** e **referências visuais da interface**. As associações a amplificadores comerciais não foram confirmadas por identificação retornada do aparelho nem por comparação acústica. As antigas estrelas e porcentagens de certeza foram removidas por não terem método de validação documentado.

## Amplificadores

TYPE USB usa índices 0–8; as posições apresentadas ao usuário são 1–9. Categorias do pedal seguem a tabela do manual transcrita na [investigação dos amps](../../../../engenharia-reversa/cube-baby/CUBE_BABY_AMPS.md).

| Posição | TYPE | Alias no perfil | Categoria do pedal | Referência visual local |
| ---: | ---: | --- | --- | --- |
| 1 | 0 | Power-Zone Clean | Clean | Koch Powertone |
| 2 | 1 | US Gold 100 Clean | Clean | Friedman BE-100 |
| 3 | 2 | Two Stone Coral OD | Overdrive | Two-Rock Coral |
| 4 | 3 | Doctor3 B | Overdrive | Dr. Z MAZ 38 |
| 5 | 4 | Cali JP A | Overdrive | Mesa/Boogie JP-2C |
| 6 | 5 | Day Tripper OD | Distortion | Vox AC30 |
| 7 | 6 | Shittcow Dist | Distortion | VHT Pittbull |
| 8 | 7 | Wo Stone Coral OD | Distortion | Two-Rock Coral / variante; associação não confirmada |
| 9 | 8 | Mr Smith Dist | Distortion | PRS Archon |

**Divergência no código:** `EquipmentCatalog` classifica Day Tripper como Overdrive, enquanto `CubeBabyProfile` e a tabela do manual usam Distortion. Isso pode afetar a categoria/filtro visual. O texto de `EquipmentCatalog` também conserva uma porcentagem de confiança para Wo Stone; ela não deve ser entendida como probabilidade medida. A revisão documental não modificou o catálogo C#.

B, A e OD em aliases não comprovam canais com esses nomes nos equipamentos comerciais apresentados.

## Gabinetes / IRs

`cabinet=0` é OFF e não possui setor. Valores 1–8 selecionam slots internos 0–7. A posição só indica onde os dados estão; a identificação do pack depende do conteúdo reconhecido.

### Packs BFI e BFIN — revisão de 03/10/2026

Nenhum conjunto de oito IRs é apresentado como “de fábrica”. O app identifica os packs **BFI** e **BFIN** por SHA-256 de cada setor completo, independentemente do slot e da presença do CubeSuite. Para aplicar um pack, o recurso local precisa existir e passar pela validação de tamanho e hash. A interface compara os oito slots e permite selecionar quais substituir, com backup, confirmação e conferência da gravação.

Os oito setores lidos do dispositivo em 03/10/2026 coincidem, byte a byte e na mesma ordem, com `CubeSuite/bin/BFIN.bin` (32.768 bytes, SHA-256 `1FABBBF9DD2797E4B4F9B43247BC2112C7285C6D8E73EDDEC0AE9191332E7E0B`). Também coincidem com o intervalo `0x69100..0x710FF` dos firmwares locais registrados no [relatório de identificação](../../../../engenharia-reversa/cube-baby/artefatos/ir-comparison-20261003/pack-identification.json). Nenhuma gravação no pedal foi feita nessa investigação.

Isso confirma **o pack**, mas não os modelos dos gabinetes. O manual local contém a tabela abaixo; outra versão do manual apresenta uma lista diferente, sem vincular cada nome ao hash de um arquivo BFIN. Não há base suficiente para escolher uma dessas listas apenas pela posição. Por isso o BFIN aparece como `Pack BFIN · IR 1` até `IR 8`, com imagem genérica e indicação de modelo não confirmado. Nomes e imagens personalizados continuam vinculados ao hash e acompanham o IR quando ele muda de slot.

A tabela abaixo conserva as referências visuais históricas do **Pack BFI**. Elas não são uma identificação acústica medida. Seus identificadores internos passaram de `factory-cab-N` para `pack-bfi-cab-N`; as imagens elegíveis são migradas por `GearImagePreferences`, mas as associações amp–gabinete do editor atual ficam apenas na sessão; o arquivo visual antigo não restaura essas associações.

| Valor de cabinet | Slot interno | Alias/referência do catálogo |
| ---: | ---: | --- |
| 0 | — | OFF / sem gabinete |
| 1 | 0 | Line 6 Vetta 2×12 |
| 2 | 1 | Marshall 1960AV 4×12 |
| 3 | 2 | Marshall 1960A T75 LA610 4×12 |
| 4 | 3 | VHT/Fryette Deliverance 2×12 |
| 5 | 4 | Soldano 2×12 |
| 6 | 5 | Peavey 5150 + Mesa Oversized 4×12 |
| 7 | 6 | JSX KT77 + Mesa Stiletto AKG C1000 4×12 |
| 8 | 7 | Diezel V30 SM57 (Soldano HR PowerAmp) 4×12 |

Os nomes de microfones/power amps fazem parte dos aliases históricos; não são metadados medidos do IR. LA610 permanece uma interpretação de cadeia de captura. Não inferir falantes do Soldano nem ano/modelo exato apenas pela foto.

Para o modelo comercial **Marshall 1960AV**, a referência do fabricante usa **G12-Vintage**, conforme [características dos gabinetes](cabinet-characteristics.md). A descrição antiga “Vintage 30” ainda está em `EquipmentCatalog.cs` e precisa de uma correção de código separada.

## Fontes e limites

[Perfil USB](../../windows/Cube.Protocol/CubeBabyProfile.cs), [catálogo de apresentação](../../windows/Cube.Protocol/EquipmentCatalog.cs), [identificação de gabinetes](../../windows/Cube.Protocol/CabinetCatalog.cs) e [imagens](../../windows/Cube.Desktop/Assets/Images/Equipment/README.md).

Características de gabinete descrevem um modelo de referência, não uma medição do arquivo. Setores personalizados ou desconhecidos não devem herdar automaticamente identidade e foto de outro pack.
