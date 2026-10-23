# Implementação e validação de Backing Tracks

Data: 22/10/2026. Direção visual: proposta B, escolhida pelo usuário.

## Implementação

- View WPF dedicada com transporte permanente, inspetor esquerdo de entrada/tomadas, ferramentas de edição e painéis recolhíveis de mixer/biblioteca. Preferências locais de dimensões e adaptação para janelas menores.
- Projetos JSON versionados .cubeproject, pasta Media, cópia das fontes, manifesto atômico, recentes, importação de sessões anteriores, relocalização e recuperação independente do salvamento manual.
- Estados explícitos de transporte, preparação, pré-contagem e passagem única A/B. Limites de captura por relógio de áudio e amostras, medição independente da monitoração e revisão de tomadas com nomes editáveis. Gravações interrompidas podem ser recuperadas ou ignoradas sem excluir arquivos.
- Fades por alças/campos, crossfade condicionado à seleção, snap, histórico de edição/tomadas e sugestão de BPM sem redefinir viewport. Pré-escuta identificável; download do YouTube separado da adição ao projeto.
- Exportação processada da mixagem ou pista/intervalo selecionados, WAV PCM 16 bits, 48 kHz estéreo, com cancelamento. Guias português/inglês atualizados e manual regenerado.

## Evidências de software

Build oficial com -CompileOnly: zero avisos e zero erros. Build de simulação, verificações existentes de regressão e verificações de recursos de tema também executados.

BackingProjectChecks.cs (--backing): 35 verificações passaram, cobrindo persistência, portabilidade, proteção de fontes/manifesto, recuperação, histórico, limites por amostras, medição e exportação/DSP. Artefatos da execução final: C:/Users/lpaferreira/AppData/Local/Temp/CubeControl-Backing-Checks/547029243bcf406ea27c49e3b48a246a.

Manual regenerado por scripts/Build-Manual.mjs: 33 páginas. Validate-Documentation.ps1 passou com 18 entradas e 80 arquivos publicados. git diff --check passou.

O auxiliar externo renderizou 1920×1080, 1366×768 e 640×480; zoom do aplicativo de 50%, 100% e 200%; temas claro/escuro; e mixer/biblioteca abertos. Essa inspeção não equivale a executar todos os fluxos por teclado ou em diferentes escalas reais do Windows.

Benchmark offline --backing-performance: oito pistas, quinze minutos e cem clipes; exportação de 900 segundos em 10,00 segundos de parede, 3,59 segundos de CPU e pico de working set de 56,3 MiB. Representa o processo de exportação, não a memória de toda a aplicação nem estabilidade de áudio ao vivo.

## Pendências para aceite integral

- Ensaios reais com Cube Baby e interface USB adicional: mono/estéreo, múltiplas pistas, latência, deriva, desconexão e estabilidade ao vivo. A enumeração local identificou somente captura e saída Realtek; não houve gravação do microfone do usuário.
- Matriz presencial de DPI do Windows 100%, 125% e 150%, teclado/Narrator, foco e alto contraste do sistema. Foram verificados recursos de tema, não toda a experiência assistiva.
- Ensaios em volume cheio e destino sem permissão. O teste de falha de salvamento verifica preservação do manifesto, mas não representa todas as falhas possíveis do sistema de arquivos.
- Download real do YouTube e pacote distribuível. O empacotamento oficial foi interrompido por HTTP 403 ao baixar https://github.com/yt-dlp/yt-dlp/releases/download/2026.08.19/yt-dlp.exe. Versões e hashes não foram alterados para contornar a falha.

A implementação não está integralmente aceita pelos critérios do plano enquanto essas evidências estiverem pendentes. Builds e dados auxiliares ficam fora do repositório em C:/CubeControl-Review/. Nenhuma publicação ou alteração no pedal foi realizada.
