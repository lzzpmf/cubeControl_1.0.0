# Backups e recuperação

## Guardar a edição
Exporte o banco A/B/C, modelos BIN e arquivos dos criadores conforme o recurso utilizado. Guarde separadamente os WAVs e músicas originais. Exportar não grava o pedal.

## Prévia interrompida
Uma prévia pode deixar um registro pendente. Ao reiniciar, o aplicativo carrega a recuperação sem reaplicar ajustes. Reconecte o mesmo aparelho e use a ação de restaurar do recurso. O perfil, banco e contexto físico precisam corresponder; não sobrescreva um estado desconhecido.

## Preservar arquivos
Backups e registros ficam em geral em `%LOCALAPPDATA%\CubeControl`, incluindo `Backups`, `AmpRecovery`, pastas de recuperação dos efeitos e arquivos `pending-*-preview.json`. Não os apague enquanto houver prévia pendente. Na edição simulada, os dados do editor ficam em `CubeControl-Simulation`.

## Restaurar fábrica
Recuperação de fábrica e substituição de slots são operações diferentes de desfazer. Leia a confirmação, confira o destino e preserve suas edições antes de executar. O histórico de presets não desfaz gravações de setores de IR ou amp.

## Confirmar o resultado
Observe a confirmação por leitura. Perda de conexão ou incompatibilidade pode impedir restauração. A resposta de identidade do modelo não é um número de série único; use o mesmo pedal e o contexto correto.

Consulte [Problemas comuns](problemas.md).
