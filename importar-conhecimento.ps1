$baseUrl = "http://localhost:5205/conhecimento"

$dados = @(
@{
    conteudo = "O oxigênio dissolvido é o parâmetro mais importante na aquicultura, essencial para as funções fisiológicas dos organismos e diretamente ligado à sobrevivência e crescimento."
    tags = "oxigenio,importancia,base"
    clienteId = 1
},
@{
    conteudo = "O oxigênio dissolvido é medido em mg/L (ppm) e em percentual de saturação."
    tags = "oxigenio,medicao,saturacao"
    clienteId = 1
},
@{
    conteudo = "100% de saturação indica água totalmente saturada de oxigênio. Abaixo disso é insaturada e acima disso é supersaturada."
    tags = "oxigenio,saturacao,agua"
    clienteId = 1
},
@{
    conteudo = "O oxigênio deve ser monitorado diariamente, preferencialmente em três períodos: manhã, tarde e noite."
    tags = "oxigenio,monitoramento,rotina"
    clienteId = 1
},
@{
    conteudo = "O nível ideal de oxigênio é acima de 5 mg/L. Valores abaixo de 3 mg/L exigem ação imediata e abaixo de 1,5 mg/L podem ser letais."
    tags = "oxigenio,critico,alerta"
    clienteId = 1
},
@{
    conteudo = "Níveis entre 65% e 80% de saturação reduzem o estresse dos organismos e melhoram o desempenho no cultivo."
    tags = "oxigenio,estresse,saturacao"
    clienteId = 1
},
@{
    conteudo = "A concentração de oxigênio é influenciada pela respiração dos organismos, fotossíntese e biomassa de microalgas e bactérias no sistema."
    tags = "oxigenio,fatores,biologia"
    clienteId = 1
},
@{
    conteudo = "A manutenção do oxigênio é feita por aeradores como paddlewheel, blower, venturi, wavemaker e chafariz."
    tags = "oxigenio,aeracao,equipamentos"
    clienteId = 1
},
@{
    conteudo = "É essencial manter equipamentos reservas e geradores para garantir oxigenação contínua em caso de falhas de energia."
    tags = "oxigenio,seguranca,energia"
    clienteId = 1
},
@{
    conteudo = "Em situações de emergência, o peróxido de hidrogênio pode ser utilizado como fonte temporária de oxigênio na água."
    tags = "oxigenio,emergencia,peroxido"
    clienteId = 1
},
@{
    conteudo = "A solubilidade do oxigênio diminui com aumento da temperatura e salinidade e também com redução da pressão atmosférica."
    tags = "oxigenio,temperatura,salinidade"
    clienteId = 1
}
)

foreach ($item in $dados) {

    $json = $item | ConvertTo-Json -Depth 3

    try {
        Invoke-RestMethod -Uri $baseUrl `
            -Method Post `
            -ContentType "application/json" `
            -Body $json

        Write-Host "OK: $($item.conteudo.Substring(0,40))..."
    }
    catch {
        Write-Host "ERRO ao enviar item"
    }
}