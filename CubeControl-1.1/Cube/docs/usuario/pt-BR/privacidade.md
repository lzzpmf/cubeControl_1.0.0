# Privacidade

[Primeiros passos](primeiros-passos.md)

Última atualização: 19 de outubro de 2026

Escopo: aplicativo Windows cubeControl 1.1.0, incluindo criador de amps e Backing Tracks.

**Responsável pelo aplicativo:** Luiz Paulo Martins Ferreira

**Contato para privacidade:** [lzpmartins@gmail.com](mailto:lzpmartins@gmail.com)

O cubeControl é um aplicativo para configurar o pedal Cube Baby por conexão USB-MIDI e reproduzir arquivos de áudio locais. Esta política explica quais informações são utilizadas, onde ficam armazenadas e como você pode controlá-las.

O responsável por esta política é o indicado acima. A referência ao nome do equipamento não identifica, por si só, seu fabricante como responsável pelo aplicativo ou pelo tratamento de dados aqui descrito.

## 1. Funcionamento local

O manual HTML funciona offline e pesquisa o conteúdo no próprio navegador, sem enviar consultas. Se o navegador permitir, guarda somente a preferência de tema na chave `cube-manual-theme` do armazenamento local. Limpar os dados do navegador remove essa preferência. Links externos e de e-mail são abertos apenas quando acionados pelo usuário.

O aplicativo funciona localmente no computador e não exige criação de conta, nome, e-mail ou cadastro para utilizar suas funções de edição. Na versão abrangida por esta política, não há envio automático de dados a servidores do desenvolvedor, publicidade, rastreamento de uso ou ferramentas de análise de comportamento.

A edição do pedal ocorre por USB-MIDI. A biblioteca de respostas de impulso incluída no aplicativo pode ser utilizada offline. Backing Tracks reproduz arquivos locais na saída de áudio escolhida no Windows, que pode ser Bluetooth; o áudio é encaminhado a essa saída, sem upload implementado pelo aplicativo. O botão de Bluetooth abre as configurações do Windows.

## 2. Informações utilizadas e suas finalidades

O aplicativo utiliza as seguintes informações para executar as funções solicitadas por você:

| Informações | Finalidade |
| --- | --- |
| Nomes e identificadores das portas MIDI disponibilizados pelo Windows e respostas de identificação do pedal | Localizar o equipamento, estabelecer a conexão e verificar a comunicação. |
| Presets, parâmetros de efeitos, dados internos de amps, respostas de impulso (IRs) e estados temporários do pedal | Exibir, editar, pré-visualizar, salvar e restaurar configurações. |
| Preferências de idioma, aparência, escala da interface e associações visuais | Manter a personalização do aplicativo. |
| Arquivos selecionados por você, como WAVs, backups e imagens PNG/JPEG | Importar IRs, restaurar configurações e personalizar imagens. Imagens escolhidas podem ser copiadas para a pasta local do aplicativo. |
| Arquivos de amp `.bin` e cópias anteriores de RAM/flash | Importar/exportar modelos do formato aceito e preservar dados para recuperação. |
| Arquivos de áudio, caminhos, backing e tentativa selecionada, dispositivos, volume e silenciar | Reproduzir o acompanhamento e gravar a guitarra durante a prática. Fontes importadas são referenciadas no local original. |
| Trechos A/B, posição, velocidade, transposição e waveform | Controlar a prática e exportar uma tentativa aparada. O estudo não é persistido nem restaurado ao reabrir. |
| Backups, datas de criação, identificação técnica do equipamento, nomes de arquivos e verificações de integridade | Permitir recuperação e conferir a integridade dos dados. |
| Mensagens de conexão, operações e erros | Exibir o estado do aplicativo e auxiliar no diagnóstico de problemas. |

Informações técnicas não identificam necessariamente uma pessoa. Entretanto, nomes de arquivos, caminhos locais, imagens ou materiais que você escolher podem conter dados pessoais. Evite incluir informações pessoais desnecessárias nesses conteúdos.

## 3. Armazenamento e conservação

Preferências, imagens personalizadas e metadados são armazenados, em geral, na pasta `%LOCALAPPDATA%\CubeControl` do seu usuário do Windows. Arquivos exportados ficam no local escolhido por você.

O estudo do Studio permanece em memória, sem restauração automática. Gravações temporárias ficam em `PracticeRecordings/<execução>` no diretório de dados ativo; descarte confirmado limpa apenas temporários próprios, e falhas preservam áudio recuperável. Arquivos antigos de playlist, metrônomo e projetos permanecem intocados. Dispositivos e buffers ficam em `studio-audio-v1.json`; retorno por software sempre inicia desligado. Pasta de downloads e IDs associados a arquivos concluídos ficam em `youtube-downloads-v1.json`, preservando preferências existentes. Preparar uma prévia por hover usa um cache limitado em memória; só Ouvir inicia som. Imagens ficam em `Images`, com associações em `gear-images-v1.json`. As cópias automáticas de recuperação de banco, IR, amp e efeitos ficam em memória, sem arquivos automáticos `pending-*-preview.json` nem retomada após reiniciar. Backups, downloads e áudios exportados ficam nos destinos escolhidos. O [guia de recuperação](recuperacao.md) descreve esses limites.

Os arquivos persistentes permanecem no computador até serem substituídos ou excluídos. Não há prazo automático geral de expiração para os backups locais. Remover o executável ou a pasta de instalação não garante a exclusão desses arquivos.

Configurações gravadas no pedal permanecem no próprio equipamento e são independentes dos arquivos armazenados no computador.

Packs pessoais são armazenados em `IrPacks`, WAVs importados de packs portáteis em `ImportedIrs` e nomes personalizados em `ir-pack-names-v1.json`, na pasta de dados do editor. A importação pode copiar os WAVs antes de salvar a montagem; descartar a montagem não equivale a excluir essas cópias.

## 4. Compartilhamento e serviços externos

O aplicativo não envia automaticamente seus presets, arquivos, informações do equipamento ou registros de diagnóstico ao desenvolvedor ou ao fabricante do pedal. Também não vende dados nem os transmite para publicidade.

Se você compartilhar voluntariamente arquivos ou capturas de tela por e-mail ou outro serviço, esse envio ocorrerá fora do fluxo local do aplicativo. Verifique o conteúdo antes de compartilhá-lo e consulte as condições do serviço utilizado.

O Windows, serviços de sincronização ou backup configurados por você e plataformas de distribuição podem realizar tratamentos próprios, sujeitos às respectivas políticas. Esta política descreve o aplicativo e não substitui as políticas desses serviços.

A busca, o download e a preparação/reprodução de prévias de áudio do YouTube, inclusive a preparação silenciosa ao manter o mouse sobre um resultado, usam ferramentas de internet para contatar serviços externos. Consultas e solicitações de mídia estão sujeitas às políticas desses serviços. Reprodução local e documentação não exigem essas solicitações.

## 5. Controle e exclusão

Você pode alterar suas preferências no aplicativo, desconectar o pedal e excluir arquivos locais ou exportados utilizando os recursos do Windows.

Antes de apagar a pasta de dados, encerre eventuais prévias de IR, restaure a prévia de amp quando aplicável e feche o aplicativo. Preserve os backups que desejar manter: a exclusão pode remover personalizações e arquivos necessários à recuperação. Preferências padrão podem ser recriadas na próxima execução. Excluir uma tentativa no Studio permite desfazer e mantém seu áudio temporário até o descarte do estudo; importações e downloads originais permanecem intocados. Salvar áudio cria um arquivo no destino escolhido.

Excluir arquivos do computador não apaga configurações já gravadas no pedal, cópias exportadas ou cópias mantidas por serviços de backup.

## 6. Segurança

O armazenamento local reduz a necessidade de transmissão de informações pela internet. O acesso aos arquivos depende das permissões e da segurança do seu computador. Não há garantia de criptografia própria para os arquivos do aplicativo.

Mantenha o Windows atualizado, proteja sua conta de usuário e controle o acesso aos backups. Nenhum mecanismo de armazenamento oferece proteção absoluta contra acesso indevido ou perda de dados.

## 7. Seus direitos e contato

Quando houver tratamento de dados pessoais pelo responsável pelo aplicativo, você poderá exercer os direitos aplicáveis previstos na Lei Geral de Proteção de Dados Pessoais (LGPD), incluindo confirmação e acesso, correção, informações sobre compartilhamento e, nas hipóteses legais, anonimização, bloqueio, exclusão, portabilidade e revogação do consentimento. Também poderá apresentar petição à ANPD, nos termos da lei.

Utilize o contato informado no início desta política para solicitações ou dúvidas. O responsável não possui acesso remoto aos arquivos que permanecem exclusivamente no seu computador; nesses casos, a gestão e a exclusão são realizadas localmente por você.

Se um atendimento exigir o envio de dados pessoais, a finalidade, a base legal aplicável, os destinatários e o período ou critério de conservação deverão ser informados nesse atendimento. O uso do aplicativo não representa consentimento genérico para finalidades adicionais.

Referência legal: [Lei nº 13.709/2018 — LGPD](https://www.planalto.gov.br/ccivil_03/_ato2015-2018/2018/lei/l13709compilado.htm).

## 8. Alterações desta política

Esta política poderá ser atualizada para refletir mudanças no aplicativo ou nas práticas de tratamento de dados. A data da revisão será indicada no início do documento. Caso sejam introduzidos recursos de coleta ou transmissão de dados, suas condições deverão ser informadas antes do tratamento correspondente, com obtenção de consentimento quando legalmente necessário.
