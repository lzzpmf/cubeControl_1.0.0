# Simulação temporária do Cube Baby

Abra `Iniciar-Simulacao.cmd`, na raiz do repositório, e clique em **Conectar**. A entrada e a saída serão `Cube Baby SIMULADO`. O título identifica permanentemente a simulação.

Essa distribuição separada permite editar, salvar e reler os presets A/B/C, além de testar as operações de IR com memória virtual. Desconectar e reconectar preserva a memória durante o mesmo processo. Fechar o aplicativo reinicia o pedal virtual; exporte os presets que quiser guardar. Não simula o áudio nem valida o USB físico. A memória de IR começa vazia, sem identificação de cabinets de fábrica.

Backups e dados do editor ficam em `%LOCALAPPDATA%\CubeControl-Simulation`, separados dos dados do pedal real. As preferências globais de tema e idioma continuam compartilhadas.

O inicializador normal não foi alterado. A compilação normal não inclui a conexão simulada nem oferece essa opção. O código é incluído exclusivamente com a propriedade `CubeSimulation=true`:

```powershell
dotnet publish Cube/windows/Cube.Desktop/Cube.Desktop.csproj -c Simulation -p:CubeSimulation=true -r win-x64 --self-contained false -o C:/CubeControl-Build/publish/CubeControl-Simulation-1.0.0
```

A primeira preparação pelo inicializador exige o SDK .NET 10 e pode precisar de internet para restaurar dependências. Após publicar, o inicializador usa a distribuição pronta. Para encerrar o uso temporário, basta voltar ao inicializador normal; nenhum ajuste no aplicativo real é necessário.
