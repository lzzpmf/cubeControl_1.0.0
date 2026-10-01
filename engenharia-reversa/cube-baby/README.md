# Cube Baby — relatórios e artefatos

[Engenharia reversa](../README.md) · [Criador de amps](../../CubeControl-1.1/Cube/docs/criador-de-amps.md)

Esta pasta reúne a investigação do Cube Baby de guitarra. Os relatórios combinam evidências do CubeSuite, leituras do pedal e ensaios registrados; o acervo Android fica separadamente em [apk-cubesuite/](../apk-cubesuite/README.md).

## Relatórios

| Documento | Conteúdo |
| --- | --- |
| [Comandos](CUBE_BABY_COMANDOS.md) | Protocolo, consultas, endereços e alcance das capturas |
| [Investigação dos amps](CUBE_BABY_AMPS.md) | Diário de ensaios, escrita/restauração e protótipos RV01–RV06 |
| [Tabelas originais](CUBE_BABY_AMPS_FABRICA.md) | Extração dos nove setores, bytes, float32 e hashes |
| [Controles e escuta](CUBE_BABY_AMPS_CONTROLES_E_ESCUTA.md) | Mapeamento de controles e observações musicais |

## Artefatos preservados

| Arquivo | Natureza |
| --- | --- |
| [Imagem original](artefatos/CUBE_BABY_FLASH_60000_6FFFF_ORIGINAL.bin) | Backup de 65536 bytes da região `0x60000–0x6FFFF`; os primeiros 36864 bytes contêm os nove amps |
| [Rockerverb Clean V1](artefatos/CUBE_BABY_ROCKERVERB_CLEAN_V1.json) | Registro do experimento em RAM, parâmetros e verificações; não é banco de presets nem formato oficial de importação |
| [Rockerverb V6](artefatos/CUBE_BABY_ROCKERVERB_V6.bin) | Arquivo de amp de 4096 bytes do protótipo RV06 descrito no diário |

Os arquivos foram preservados integralmente. O caminho de áudio dentro do JSON pertence ao ensaio original e não é uma dependência deste checkout. Os hashes e limites de interpretação constam nos relatórios. A presença desses arquivos não aplica configurações ao pedal automaticamente.
