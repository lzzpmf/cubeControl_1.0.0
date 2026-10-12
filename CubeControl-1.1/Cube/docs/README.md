# Documentação do cubeControl

## Para usuários

Os guias publicados têm fontes únicas em `usuario/pt-BR` e `usuario/en-US`. A opção **Documentação**, em Dispositivo e ajuda, oferece leitura offline, categorias, busca e sumário. O build copia somente o catálogo, guias e manuais para o pacote; não edite cópias no pacote.

| Português | English |
| --- | --- |
| [Primeiros passos](usuario/pt-BR/primeiros-passos.md) | [Getting started](usuario/en-US/primeiros-passos.md) |
| [Presets, gravação e arquivos](usuario/pt-BR/presets.md) | [Presets, saving and files](usuario/en-US/presets.md) |
| [Pedaleira e efeitos](usuario/pt-BR/pedaleira.md) | [Pedalboard and effects](usuario/en-US/pedaleira.md) |
| [IRs, gabinetes e associações](usuario/pt-BR/irs.md) | [IRs, cabinets and associations](usuario/en-US/irs.md) |
| [Criador de amps](usuario/pt-BR/criador-amps.md) | [Amp creator](usuario/en-US/criador-amps.md) |
| [Usar os criadores de efeitos](usuario/pt-BR/criadores-efeitos.md) | [Using effect creators](usuario/en-US/criadores-efeitos.md) |
| [Criador de chorus](usuario/pt-BR/criador-chorus.md) | [Chorus creator](usuario/en-US/criador-chorus.md) |
| [Criador de phaser](usuario/pt-BR/criador-phaser.md) | [Phaser creator](usuario/en-US/criador-phaser.md) |
| [Criador de delay](usuario/pt-BR/criador-delay.md) | [Delay creator](usuario/en-US/criador-delay.md) |
| [Criador de reverb](usuario/pt-BR/criador-reverb.md) | [Reverb creator](usuario/en-US/criador-reverb.md) |
| [Backing Tracks](usuario/pt-BR/backing-tracks.md) | [Backing Tracks](usuario/en-US/backing-tracks.md) |
| [Simulação](usuario/pt-BR/simulacao.md) | [Simulation](usuario/en-US/simulacao.md) |
| [Problemas comuns](usuario/pt-BR/problemas.md) | [Troubleshooting](usuario/en-US/problemas.md) |
| [Backups e recuperação](usuario/pt-BR/recuperacao.md) | [Backups and recovery](usuario/en-US/recuperacao.md) |
| [Privacidade](usuario/pt-BR/privacidade.md) | [Privacy](usuario/en-US/privacidade.md) |

## Desenvolvimento, referências e histórico

### Desenvolvimento

- [arquitetura](desenvolvimento/arquitetura.md)
- [Diagnósticos e pesquisa com hardware](../windows/README.md#diagnósticos-e-pesquisa-com-hardware)
- [wpf-ui-integracoes](desenvolvimento/wpf-ui-integracoes.md)
- [windows-interaction](desenvolvimento/windows-interaction.md)
- [snackbars](desenvolvimento/snackbars.md)
- [revisao-documental](desenvolvimento/revisao-documental.md)
### Referencias

- [editores-graficos-de-efeitos](referencias/editores-graficos-de-efeitos.md)
- [criador-de-amps](referencias/criador-de-amps.md)
- [cubecontrol-ir-e-fabrica](referencias/cubecontrol-ir-e-fabrica.md)
- [ampsEirs](referencias/ampsEirs.md)
- [cabinet-characteristics](referencias/cabinet-characteristics.md)
- [effect-descriptions](referencias/effect-descriptions.md)
### Historico

- [physical-switch-investigation](historico/physical-switch-investigation.md)
- [analise-protocolo-windows](historico/analise-protocolo-windows.md)
- [historico-interface](historico/historico-interface.md)
- [guia-de-uso histórico](historico/guia-de-uso.md)
- [backing-tracks](historico/backing-tracks.md)
- [simulacao-temporaria](historico/simulacao-temporaria.md)
- [Plano de QA](desenvolvimento/qa-test-plan.md)

[Design system](DESIGN_SYSTEM.md) permanece no caminho normativo. [Guia Windows](../windows/README.md) contém build e testes.

## Pesquisa, terceiros e manuais

[Engenharia reversa](../../../engenharia-reversa/README.md) conserva relatórios, fontes Android e artefatos. Documentos de terceiros, licenças e READMEs de componentes permanecem junto às fontes que descrevem. Arquivos em `tmp` e `output` são artefatos de trabalho, não documentos publicados.

Manuais originais e adaptados, PDFs e fontes Markdown, ficam em [Assets/Documents](../windows/Cube.Desktop/Assets/Documents/README.md), separados por origem e idioma.

[Inventário e classificação](inventario-documental.md) registra os destinos e as regras de preservação.

[Validação da central](desenvolvimento/validacao-documentacao.md) registra os testes e seu alcance.

## Manutenção

1. Atualize os guias pt-BR e en-US junto com mudanças observáveis no produto. Preserve a distinção entre edição local, prévia RAM e gravação persistente.
2. Cadastre documentos publicáveis em `usuario/catalogo.v1.json`, com ID estável, categoria e versões por idioma. A central exibe somente esse catálogo.
3. Use links relativos entre guias e âncoras de títulos; não ligue um guia publicado a caminhos exclusivos do checkout.
4. Preserve evidências de pesquisa, autoria e documentos de terceiros. Não transforme ensaios antigos em validações atuais.
5. Confira links do acervo e execute a validação documental e de pacote antes de distribuir.
