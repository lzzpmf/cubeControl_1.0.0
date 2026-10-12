# Backups e recuperação

## Guardar a edição
Exporte o banco A/B/C, modelos BIN e arquivos dos criadores conforme o recurso utilizado. Guarde separadamente os WAVs e músicas originais. Exportar não grava o pedal.

## Prévia interrompida
O estado anterior fica em memória durante a execução atual. Se a conexão cair, reconecte o mesmo aparelho e use a ação de restaurar do recurso, respeitando as verificações de perfil e contexto. Reiniciar o aplicativo descarta os snapshots; não há retomada automática de uma prévia da execução anterior. O fechamento normal tenta restaurar as prévias de efeitos, amp e IR e informa falhas antes de oferecer fechar mesmo assim.

## Preservar arquivos
As cópias automáticas de banco, IR e amp mantidas pelo editor são temporárias, em memória. Para conservar dados entre execuções, use as exportações explícitas e guarde os arquivos no destino escolhido. Preferências e imagens ficam em `%LOCALAPPDATA%\CubeControl`; na edição simulada, os dados do editor usam `CubeControl-Simulation`. Arquivos de recuperação antigos não são carregados automaticamente.

## Restaurar fábrica
Recuperação de fábrica e substituição de slots são operações diferentes de desfazer. Leia a confirmação, confira o destino e preserve suas edições antes de executar. O histórico de presets não desfaz gravações de setores de IR ou amp.

A restauração dos presets de fábrica procura `%USERPROFILE%\Downloads\CubeSuite\CubeSuite.exe` e `bin\BFI.bin` nessa instalação. O aplicativo verifica versões específicas por tamanho/hash; arquivos ausentes ou não reconhecidos deixam o recurso indisponível. Não há seletor de origem nesse fluxo. Restaurar IRs originais verifica separadamente os packs BFI/BFIN disponíveis na mesma pasta `bin`. Esses recursos externos não acompanham o pacote do cubeControl; `FactoryAmps.bin` não os substitui.

## Reinstalar ou atualizar firmware
Em Dispositivo e ajuda, desconecte a sessão MIDI do editor antes de instalar. Selecione um `.up` da lista ou escolha um arquivo; atualizar a lista não grava o pedal. O aplicativo valida o formato, mas isso não comprova que a variante seja adequada ao seu aparelho. A instalação não está disponível na edição simulada.

Exporte os dados que quiser conservar: presets e IRs podem ser substituídos. Feche o `flash_loader` e entre no modo flash ligando o pedal conectado ao PC enquanto segura o terceiro footswitch, conforme a confirmação do aplicativo. Ao confirmar, o Windows pede permissão de administrador. Não desligue nem desconecte durante a gravação. O progresso e eventuais erros aparecem no editor; não confunda essa operação com restaurar apenas presets ou IRs. Se a gravação for interrompida, observe a mensagem de erro antes de tentar novamente; não há promessa de restauração automática do firmware anterior.

## Confirmar o resultado
Observe a confirmação por leitura. Perda de conexão ou incompatibilidade pode impedir restauração. A resposta de identidade do modelo não é um número de série único; use o mesmo pedal e o contexto correto.

Consulte [Problemas comuns](problemas.md).
