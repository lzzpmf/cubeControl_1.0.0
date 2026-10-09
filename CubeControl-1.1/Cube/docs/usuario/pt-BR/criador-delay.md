# Criador de delay

## Preparar a prévia
Na Pedaleira, abra Delay → Criador de delay. Ative a seção delay/modulação, desligue chorus/phaser, selecione o mesmo preset físico no aplicativo e use Ler delay ativo. Um perfil ou contexto incompatível impede iniciar a prévia; não tente contornar essa verificação.

## Controles
O gráfico oferece profundidade e velocidade de modulação temporal, tempo em amostras internas e resolução fina de realimentação/mistura. Profundidade e velocidade são percentuais do original: profundidade zero elimina a oscilação, velocidade zero congela sua fase. A velocidade efetiva depende do tempo. Mistura interna 50 mantém direto e eco integrais; não é uma soma complementar wet/dry. A equivalência do tempo em milissegundos não foi comprovada por áudio.

## Arquivos e restauração
Importar `.delay.json` prepara ajustes locais; Aplicar ajustes envia para a prévia ativa. Exportar não escreve no pedal. Não há gravação persistente em flash nem prévia desse efeito no dispositivo simulado.

Restaurar delay, desconexão normal, releitura e fechamento normal tentam devolver os parâmetros originais. Perda de conexão ou mudança de contexto interrompem os envios; uma tentativa de restauração não equivale a confirmação de sucesso.

Após reiniciar, a recuperação pendente é carregada sem reaplicar ajustes. Reconecte o mesmo aparelho, restabeleça o contexto correspondente e use Restaurar delay. Preserve `pending-delay-preview.json` e `DelayRecovery` na pasta de dados até resolver a recuperação.

Veja [Criadores de efeitos](criadores-efeitos.md) e [Backups e recuperação](recuperacao.md).
