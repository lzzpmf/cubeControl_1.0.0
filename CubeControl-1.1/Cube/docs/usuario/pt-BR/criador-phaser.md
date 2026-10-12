# Criador de phaser

## Preparar a prévia
Na Pedaleira, abra Phaser → Criador de phaser. Ative phaser e a seção delay/modulação no preset físico, selecione o mesmo A/B/C no aplicativo e use Ler phaser ativo. Um perfil ou contexto incompatível impede iniciar a prévia; não tente contornar essa verificação.

## Controles
Velocidade e extremos de frequência estimados, nível e número de seções. O extremo inferior deve ser menor ou igual ao superior. Valores estimados não são medições acústicas.

## Arquivos e restauração
Importar `.phaser.json` prepara ajustes locais; Aplicar ajustes envia para a prévia ativa. Exportar não escreve no pedal. Não há gravação persistente em flash nem prévia desse efeito no dispositivo simulado.

Restaurar phaser, desconexão normal, releitura e fechamento normal tentam devolver os parâmetros originais. Perda de conexão ou mudança de contexto interrompem os envios; uma tentativa de restauração não equivale a confirmação de sucesso.

O snapshot de recuperação permanece apenas na memória desta execução. Se a conexão cair, reconecte o mesmo aparelho e use Restaurar phaser, respeitando o contexto validado pelo aplicativo. Fechar ou reiniciar o aplicativo descarta esse snapshot; não há carregamento automático de arquivos de recuperação antigos. Exporte os ajustes que desejar guardar.

Veja [Criadores de efeitos](criadores-efeitos.md) e [Backups e recuperação](recuperacao.md).
