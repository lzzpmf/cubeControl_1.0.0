# Simulação do Cube Baby

[Documentação](README.md) · Revisão: 01/10/2026

A simulação é uma compilação separada que inclui `FakeConnection`. Para executá-la, use PowerShell **na raiz do repositório**:

```powershell
.\scripts\Build-CubeControl.ps1 -Simulation -Start
```

A entrada/saída aparece como **Cube Baby SIMULADO** e o título identifica a simulação. O fluxo automático tenta conectar; o botão de conexão também está disponível. A compilação normal não inclui a conexão falsa nem oferece troca para simulação em execução.

## Alcance

Permite editar, salvar e reler A/B/C e executar operações de IR em memória virtual. Desconectar/reconectar preserva essa memória durante o mesmo processo; fechar o aplicativo reinicia o pedal virtual. Exporte os presets que quiser guardar.

Não simula o áudio do DSP nem valida USB físico, apagamento real ou retenção sem energia. A memória de IR inicia vazia, sem identificação de gabinetes de fábrica. Leitura, prévia e gravação de amps no dispositivo ficam desabilitadas; importação/exportação local de amps continua possível. Backing Tracks usa o áudio real do Windows mesmo nessa compilação.

## Dados e publicação

Os dados do editor usam `%LOCALAPPDATA%\CubeControl-Simulation`. Tema, idioma e **zoom** continuam compartilhados em `%LOCALAPPDATA%\CubeControl`.

O inicializador `CubeControl-1.1/Iniciar-Simulacao.cmd` localiza `scripts/Build-CubeControl.ps1` relativamente à própria pasta e passa `-Simulation -Start`. Ele usa o fluxo único de build antes de abrir `C:\CubeControl-Build\publish\CubeControl-Simulation-1.1.0\CubeControl.exe`. Esse fluxo exige o SDK .NET 10. Para apenas gerar o pacote, omita `-Start`; para mudar a raiz de saída, use `-OutputRoot` com uma pasta externa ao projeto.

O fluxo único está no [guia Windows](../windows/README.md). O script fornece `CubeSimulation=true`, que inclui o transporte simulado; não é necessário montar comandos de compilação manualmente. Os intermediários e o pacote final ficam fora do projeto, assim como na edição normal.
