# Criador de reverb

## Preparar a prévia
Na Pedaleira, abra Reverb → Criador de reverb. Ative reverb e a seção reverb/cabinet, desligue a modulação, selecione o mesmo preset físico no aplicativo e use Ler reverb ativo. Um perfil ou contexto incompatível impede iniciar a prévia; não tente contornar essa verificação.

## Controles
Sinal direto, sinal reverberado, realimentação, retorno das linhas e amortecimento. Ajuste gradualmente dentro das faixas do editor.

## Arquivos e restauração
Importar `.reverb.json` prepara ajustes locais; Aplicar ajustes envia para a prévia ativa. Exportar não escreve no pedal. Não há gravação persistente em flash nem prévia desse efeito no dispositivo simulado.

Restaurar reverb, desconexão normal, releitura e fechamento normal tentam devolver os parâmetros originais. Perda de conexão ou mudança de contexto interrompem os envios; uma tentativa de restauração não equivale a confirmação de sucesso.

Após reiniciar, a recuperação pendente é carregada sem reaplicar ajustes. Reconecte o mesmo aparelho, restabeleça o contexto correspondente e use Restaurar reverb. Preserve `pending-reverb-preview.json` e `ReverbRecovery` na pasta de dados até resolver a recuperação.

Veja [Criadores de efeitos](criadores-efeitos.md) e [Backups e recuperação](recuperacao.md).
