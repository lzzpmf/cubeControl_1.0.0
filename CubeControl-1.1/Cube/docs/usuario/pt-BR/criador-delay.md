# Criador de delay

## Preparar a prévia
Na Pedaleira, abra Delay → Criador de delay. Ative a seção delay/modulação, desligue chorus/phaser, selecione o mesmo preset físico no aplicativo e use Ler delay ativo. Um perfil ou contexto incompatível impede iniciar a prévia; não tente contornar essa verificação.

## Controles
O gráfico oferece profundidade e velocidade de modulação temporal, tempo em amostras internas e resolução fina de realimentação/mistura. Profundidade e velocidade são percentuais do original: profundidade zero elimina a oscilação, velocidade zero congela sua fase. A velocidade efetiva depende do tempo. Mistura interna 50 mantém direto e eco integrais; não é uma soma complementar wet/dry. A equivalência do tempo em milissegundos não foi comprovada por áudio.

## Arquivos e restauração
Importar `.delay.json` prepara ajustes locais; Aplicar ajustes envia para a prévia ativa. Exportar não escreve no pedal. Não há gravação persistente em flash nem prévia desse efeito no dispositivo simulado.

Restaurar delay, desconexão normal, releitura e fechamento normal tentam devolver os parâmetros originais. Perda de conexão ou mudança de contexto interrompem os envios; uma tentativa de restauração não equivale a confirmação de sucesso.

O snapshot de recuperação permanece apenas na memória desta execução. Se a conexão cair, reconecte o mesmo aparelho e use Restaurar delay, respeitando o contexto validado pelo aplicativo. Fechar ou reiniciar o aplicativo descarta esse snapshot; não há carregamento automático de arquivos de recuperação antigos. Exporte os ajustes que desejar guardar.

Veja [Criadores de efeitos](criadores-efeitos.md) e [Backups e recuperação](recuperacao.md).
