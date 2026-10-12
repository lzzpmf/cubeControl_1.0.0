# Criador de phaser

## Preparar a prévia
Selecione fisicamente o modo PRESET e o A/B/C desejado no pedal. Na Pedaleira, abra o criador e use Ler phaser ativo para iniciar a prévia. O aplicativo seleciona na interface o preset físico detectado. A preparação seleciona phaser e ativa a seção delay/modulação quando necessário. Essa preparação pode alterar parâmetros ao vivo, sem salvar o banco. Um perfil ou contexto incompatível impede iniciar a prévia; consulte [Criadores de efeitos](criadores-efeitos.md).

## Controles
Velocidade e extremos de frequência estimados, nível e número de seções. O extremo inferior deve ser menor ou igual ao superior. Valores estimados não são medições acústicas.

## Arquivos e restauração
Importar `.phaser.json` prepara ajustes locais; Aplicar ajustes envia para a prévia ativa. Exportar não escreve no pedal. Não há gravação persistente em flash nem prévia desse efeito no dispositivo simulado.

Restaurar phaser, desconexão normal, releitura e fechamento normal tentam devolver os parâmetros originais. Perda de conexão ou mudança de contexto interrompem os envios; uma tentativa de restauração não equivale a confirmação de sucesso.

O snapshot de recuperação permanece apenas na memória desta execução. Se a conexão cair, reconecte o mesmo aparelho e use Restaurar phaser, respeitando o contexto validado pelo aplicativo. Fechar ou reiniciar o aplicativo descarta esse snapshot; não há carregamento automático de arquivos de recuperação antigos. Exporte os ajustes que desejar guardar.

Veja [Criadores de efeitos](criadores-efeitos.md) e [Backups e recuperação](recuperacao.md).
