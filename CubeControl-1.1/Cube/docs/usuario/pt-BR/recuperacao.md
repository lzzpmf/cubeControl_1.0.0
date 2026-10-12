# Backups e recuperação

## Guardar a edição
Exporte o banco A/B/C, modelos BIN e arquivos dos criadores conforme o recurso utilizado. Guarde separadamente os WAVs e músicas originais. Exportar não grava o pedal.

## Prévia interrompida
O estado anterior fica em memória durante a execução atual. Se a conexão cair, reconecte o mesmo aparelho e use a ação de restaurar do recurso, respeitando as verificações de perfil e contexto. Reiniciar o aplicativo descarta os snapshots; não há retomada automática de uma prévia da execução anterior. O fechamento normal tenta restaurar as prévias de efeitos, amp e IR e informa falhas antes de oferecer fechar mesmo assim.

## Preservar arquivos
As cópias automáticas de banco, IR e amp mantidas pelo editor são temporárias, em memória. Para conservar dados entre execuções, use as exportações explícitas e guarde os arquivos no destino escolhido. Preferências e imagens ficam em `%LOCALAPPDATA%\CubeControl`; na edição simulada, os dados do editor usam `CubeControl-Simulation`. Arquivos de recuperação antigos não são carregados automaticamente.

## Restaurar fábrica
Recuperação de fábrica e substituição de slots são operações diferentes de desfazer. Leia a confirmação, confira o destino e preserve suas edições antes de executar. O histórico de presets não desfaz gravações de setores de IR ou amp.

## Confirmar o resultado
Observe a confirmação por leitura. Perda de conexão ou incompatibilidade pode impedir restauração. A resposta de identidade do modelo não é um número de série único; use o mesmo pedal e o contexto correto.

Consulte [Problemas comuns](problemas.md).
