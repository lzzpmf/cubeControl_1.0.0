# Organização dos recursos

- `Images/Equipment`: artes usadas pelo app, separadas em Amps, Cabinets, Effects, Devices e Microphones.
- `Images/Equipment/Cabinets/Selection`: opções da seleção de imagem ao gravar um IR. Esta lista permanece separada das artes automáticas dos modelos.
- `Images/Controls`: controles utilizados, por Knobs, Faders, Switches e BackingTrack. Os arquivos oficiais são recursos WPF embutidos; os controles auxiliares são arquivos externos.
- `Images/Icons`: ícones embutidos e ícone do executável.
- `Archive/Images`: originais preservados. O projeto distribui imagens elegíveis de Cabeçotes, Caixas, Pedais, Guitarras, Dispositivos, Equipment e Racks em `Images/Catalog`, conservando subpastas e excluindo nomes de controles. A pasta `Archive` em si não acompanha o pacote. Duplicatas permanecem separadas para preservar sua origem.
- `Documents`: manuais preservados. O projeto também copia o catálogo e os guias pt-BR/en-US de `docs/usuario` para `Assets/Documents`; as fontes únicas dos guias ficam fora de Assets.
- `RawWaves`: cinco gravações cruas de guitarra, copiadas para a pré-escuta de IRs.
- `Firmwares`, `ImpulseResponses`, `FactoryAmps.bin` e `IrPacks.v1.json`: caminhos preservados.

`asset-moves.json` registra origem, destino (relativos a Assets) e SHA-256 de cada arquivo movido em 08/10/2026. Nenhuma imagem foi renomeada ou alterada. O README de equipamentos teve seus links atualizados; seus hashes original e final estão registrados separadamente. `AssetPaths` mantém o mapeamento explícito dos arquivos externos, sem depender da busca por nomes em várias pastas.

Imagens personalizadas permanecem em `DataDirectory/Images`, fora deste acervo. A reorganização de pastas preservou o mapeamento de arquivos; consulte `IrPackCatalog` para os IDs atuais e `GearImagePreferences` para a migração das imagens. As associações amp–gabinete do editor ficam na sessão, separadas das imagens persistidas.
