# Criador de amps

## Preparar a edição
Escolha um amp base ou importe um BIN válido de 4096 bytes. Esse formato não é WAV de gabinete, JSON de presets nem modelo NAM. Offline, a importação prepara uma edição local.

## Ajustar e ouvir
A escolha da origem na interface tenta iniciar a prévia quando o pedal está disponível. Ajustes disparam prévia automática. A prévia altera RAM compartilhada do amp ativo: selecionar a origem não comprova qual amp está ativo fisicamente. Trocar o amp no pedal pode recarregar sua RAM.

Ganho e volume do criador alteram tabelas internas; são diferentes dos controles GAIN e VOLUME do preset. EQ Clássico/Moderno são formas de apresentação. Ajuste gradualmente e use os limites apresentados.

## Edição precisa e perfis de resposta
Clique no valor ou pressione Enter para digitar; Enter confirma e Escape ou sair do campo descarta. Restaurar banda recupera os parâmetros e coeficientes daquele EQ. A marca junto ao valor indica diferença da referência carregada, não do conteúdo de fábrica.

Resposta oferece dois painéis de capacitores com perfil e intensidade. Original e intensidade zero preservam a referência. Os perfis são propostas para escuta, não uma simulação elétrica calibrada. Restaurar original afeta somente o painel escolhido. Importar, ler ou carregar estabelece nova referência, sem tentar inferir perfis do som importado.

Duplo clique no knob ou fader restaura a referência carregada; Backspace com foco no controle faz o mesmo. Na EQ, restaura a banda inteira; em Resposta, o perfil e sua intensidade originais.

## Exportar, restaurar e gravar
- Exportar BIN guarda sua edição no computador.
- Restaurar tenta devolver a RAM capturada no início da prévia.
- Salvar no pedal substitui o setor do slot indicado na confirmação. Essa operação é persistente e não é desfeita pelo histórico de presets.

Mantenha backups e leia [Recuperação](recuperacao.md). Na edição simulada, leitura, prévia e gravação de amp no dispositivo ficam desabilitadas; arquivos locais continuam disponíveis.

Depois de uma gravação bem-sucedida, selecione outro amp no pedal e volte para recarregar o slot. A confirmação de bytes por leitura não comprova retenção após desligar nem a recarga física do som.
