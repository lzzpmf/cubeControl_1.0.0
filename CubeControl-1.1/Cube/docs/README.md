# Documentação do cubeControl

[Repositório](../../../README.md) · Revisão: 01/10/2026

Este índice organiza toda a documentação Markdown do repositório. A aplicação permanece em `CubeControl-1.1/`; o acervo do APK e os relatórios/artefatos de pesquisa ficam em [engenharia-reversa/](../../../engenharia-reversa/README.md). Comece pelos guias; consulte registros históricos para entender a origem e os limites das descobertas.

## Uso do aplicativo

| Documento | Conteúdo |
| --- | --- |
| [Guia de uso](guia-de-uso.md) | Conexão, A/B/C, gravação, importação/exportação, IRs e problemas comuns |
| [Criador de amps](criador-de-amps.md) | Arquivos de amp, prévia em RAM, controles, flash e limitações |
| [Criador de phaser experimental](../../../engenharia-reversa/cube-baby/CUBE_BABY_PHASER.md) | Controles internos, prévia RAM, arquivos locais e validação sem guitarra |
| [Criador de chorus experimental](../../../engenharia-reversa/cube-baby/CUBE_BABY_CHORUS.md) | Sete controles, prévia RAM, arquivos locais, recuperação e evidências USB |
| [Criador de reverb experimental](../../../engenharia-reversa/cube-baby/CUBE_BABY_REVERB.md) | Cinco ajustes independentes, prévia RAM, recuperação, modelo e evidências USB |
| [Criador de delay experimental](../../../engenharia-reversa/cube-baby/CUBE_BABY_DELAY.md) | Profundidade/velocidade de modulação, controles finos, prévia RAM e recuperação |
| [Backing Tracks](backing-tracks.md) | MP3/WAV, saídas de áudio, trechos, sequência e exportação |
| [Simulação](simulacao-temporaria.md) | Preparação, memória virtual e recursos não simulados |
| [Privacidade](POLITICA-DE-PRIVACIDADE.md) | Responsável, contato, dados locais e exclusão |

## Desenvolvimento e manutenção

| Documento | Conteúdo |
| --- | --- |
| [Guia Windows](../windows/README.md) | SDK, build único, opções, destinos externos e testes |
| [Arquitetura](arquitetura.md) | Projetos, comunicação, persistência, recursos e formatos |
| [Design system](DESIGN_SYSTEM.md) | Padrões atuais de interface e critérios de revisão |
| [Integrações WPF UI](wpf-ui-integracoes.md) | Componentes utilizados e propostas ainda não implementadas |
| [Interação no Windows](windows-interaction.md) | Zoom, rolagem, animação, Mica e validação |
| [Notificações](snackbars.md) | Eventos, severidade e duração dos snackbars |
| [Localização](../windows/Cube.Desktop/Localization/README.md) | Catálogos e inclusão de traduções |
| [Imagens de equipamentos](../windows/Cube.Desktop/Assets/Equipment/README.md) | Arquivos padrão, identidades e personalização |
| [Revisão e manutenção documental](revisao-documental.md) | Cobertura dos arquivos, fontes, verificações e pendências |

## Referências técnicas e de apresentação

| Documento | Como usar |
| --- | --- |
| [Protocolo e comandos](../../../engenharia-reversa/cube-baby/CUBE_BABY_COMANDOS.md) | Distingue capturas, implementação e comandos genéricos não comprovados |
| [IR e recursos de fábrica](cubecontrol-ir-e-fabrica.md) | Endereços, formato WAV/IR, backups e origem dos recursos |
| [Catálogo de amps e gabinetes](ampsEirs.md) | Índices, aliases e referências visuais; não certifica modelos comerciais |
| [Características dos gabinetes](cabinet-characteristics.md) | Referências dos fabricantes, separadas da identidade do IR |
| [Descrições dos efeitos](effect-descriptions.md) | Textos explicativos da interface e seu alcance |
| [Tabelas originais dos nove amps](../../../engenharia-reversa/cube-baby/CUBE_BABY_AMPS_FABRICA.md) | Dados binários completos, decimais float32 e hashes |
| [Controles de amp e escuta](../../../engenharia-reversa/cube-baby/CUBE_BABY_AMPS_CONTROLES_E_ESCUTA.md) | Grupos, limites implementados e observações musicais |

## Pesquisa e histórico

| Documento | Situação |
| --- | --- |
| [Investigação dos amps](../../../engenharia-reversa/cube-baby/CUBE_BABY_AMPS.md) | Diário de ensaios, substituição/restauração e protótipos RV01–RV06 |
| [Investigação dos footswitches](physical-switch-investigation.md) | Experimento não validado; monitoramento não iniciado no aplicativo atual |
| [Análise inicial Android/BLE](analise-protocolo-windows.md) | Registro anterior à aplicação WPF/USB; inclui outros produtos |
| [Histórico de interface](historico-interface.md) | Diretrizes antigas, inclusive alternativas substituídas |
| [README do jsoup](../../../engenharia-reversa/apk-cubesuite/resources/META-INF/README.md) | Documento de terceiro, preservado sem reescrita |

Os [README da raiz](../../../README.md) e [da aplicação](../../README.md) são pontos de entrada. Resultados antigos de testes são evidências históricas, não verificações executadas nesta revisão. Caminhos de capturas e ferramentas ausentes estão identificados nos registros.

## Convenções

- **Atual:** conferido no código desta árvore em 01/10/2026.
- **Histórico:** descreve uma versão, ensaio ou máquina anterior.
- **Hipótese:** interpretação ainda sem comprovação suficiente.
- **Pendente:** divergência no produto ou validação necessária; editar documentação não implica corrigir o código.

O código define o comportamento implementado; dumps confirmam bytes; relatos de escuta descrevem a percepção naquela condição. Nenhuma dessas fontes, isoladamente, comprova todo o funcionamento do DSP.
