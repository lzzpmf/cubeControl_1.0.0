# Criador de chorus

## Preparar a prévia
Na Pedaleira, abra Chorus → Criador de chorus. Ative chorus no preset físico em modo PRESET e selecione o mesmo A/B/C no aplicativo. Use Ler chorus ativo. Um perfil ou contexto incompatível impede iniciar a prévia; não tente contornar essa verificação.

## Controles
Profundidade, velocidade estimada, mistura do ramo processado, realimentação, forma de onda e ganhos dos ramos atrasado e combinado. Mistura do ramo processado não equivale a porcentagem wet.

## Arquivos e restauração
Importar `.chorus.json` prepara ajustes locais; Aplicar ajustes envia para a prévia ativa. Exportar não escreve no pedal. Não há gravação persistente em flash nem prévia desse efeito no dispositivo simulado.

Restaurar chorus, desconexão normal, releitura e fechamento normal tentam devolver os parâmetros originais. Perda de conexão ou mudança de contexto interrompem os envios; uma tentativa de restauração não equivale a confirmação de sucesso.

Após reiniciar, a recuperação pendente é carregada sem reaplicar ajustes. Reconecte o mesmo aparelho, restabeleça o contexto correspondente e use Restaurar chorus. Preserve `pending-chorus-preview.json` e `ChorusRecovery` na pasta de dados até resolver a recuperação.

Veja [Criadores de efeitos](criadores-efeitos.md) e [Backups e recuperação](recuperacao.md).
