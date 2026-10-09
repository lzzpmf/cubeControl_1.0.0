# Organização dos recursos

- `Images/Equipment`: artes usadas pelo app, separadas em Amps, Cabinets, Effects, Devices e Microphones.
- `Images/Equipment/Cabinets/Selection`: opções da seleção de imagem ao gravar um IR. Esta lista permanece separada das artes automáticas dos modelos.
- `Images/Controls`: controles utilizados, por Knobs, Faders, Switches e BackingTrack. Os arquivos oficiais são recursos WPF embutidos; os controles auxiliares são arquivos externos.
- `Images/Icons`: ícones embutidos e ícone do executável.
- `Archive/Images`: alternativas e referências preservadas, sem distribuição no build/publicação. Duplicatas permanecem separadas para preservar sua origem.
- `Documents`: manuais e tabelas de referência. O manual PDF acompanha o aplicativo.
- `Firmwares`, `ImpulseResponses`, `FactoryAmps.bin` e `IrPacks.v1.json`: caminhos preservados.

`asset-moves.json` registra origem, destino (relativos a Assets) e SHA-256 de cada arquivo movido em 08/10/2026. Nenhuma imagem foi renomeada ou alterada. O README de equipamentos teve seus links atualizados; seus hashes original e final estão registrados separadamente. `AssetPaths` mantém o mapeamento explícito dos arquivos externos, sem depender da busca por nomes em várias pastas.

Imagens personalizadas permanecem em `DataDirectory/Images`, fora deste acervo. IDs e associações de equipamentos continuam os mesmos.
