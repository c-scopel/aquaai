$baseUrl = "http://localhost:5205/conhecimento"

$dados = @(

@{
    conteudo = "A receita na aquicultura é calculada com base na quantidade de animais produzidos multiplicada pelo peso total e preço de venda."
    tags = "receita,aquicultura,producao,preco venda"
    clienteId = 1
},
@{
    conteudo = "A receita líquida é obtida após desconto de impostos e comissões sobre a receita bruta."
    tags = "receita liquida,impostos,comissao,financeiro"
    clienteId = 1
},
@{
    conteudo = "Os principais custos de produção incluem ração, alevinos, energia, mão de obra e despesas operacionais."
    tags = "custos producao,racao,alevinos,energia,mao de obra"
    clienteId = 1
},
@{
    conteudo = "A ração representa um dos maiores custos operacionais na aquicultura, podendo impactar diretamente a rentabilidade."
    tags = "racao,custo alto,impacto financeiro"
    clienteId = 1
},
@{
    conteudo = "O custo com energia elétrica é relevante em sistemas intensivos devido ao uso contínuo de aeradores e equipamentos."
    tags = "energia,custo operacional,aeracao"
    clienteId = 1
},
@{
    conteudo = "O lucro bruto é calculado pela diferença entre receita líquida e custos diretos de produção."
    tags = "lucro bruto,financeiro,indicadores"
    clienteId = 1
},
@{
    conteudo = "O lucro operacional considera também despesas fixas como aluguel, transporte e custos administrativos."
    tags = "lucro operacional,despesas fixas,financeiro"
    clienteId = 1
},
@{
    conteudo = "A biomassa produzida é um indicador-chave de desempenho e está diretamente relacionada à receita do sistema."
    tags = "biomassa,producao,indicador desempenho"
    clienteId = 1
},
@{
    conteudo = "A densidade de estocagem influencia diretamente os custos, a produção total e a eficiência do sistema."
    tags = "densidade,producao,custo,eficiencia"
    clienteId = 1
},
@{
    conteudo = "O custo por quilo produzido é um dos principais indicadores para avaliar a viabilidade econômica do cultivo."
    tags = "custo por kg,viabilidade,indicador financeiro"
    clienteId = 1
},
@{
    conteudo = "O uso de insumos como ração, aditivos e alevinos deve ser controlado para evitar aumento excessivo dos custos variáveis."
    tags = "insumos,custos variaveis,controle financeiro"
    clienteId = 1
},
@{
    conteudo = "Os custos fixos incluem mão de obra, manutenção, aluguel e despesas administrativas do sistema."
    tags = "custos fixos,mao de obra,manutencao,gestao"
    clienteId = 1
},
@{
    conteudo = "O fluxo de caixa permite acompanhar entradas e saídas financeiras ao longo do tempo, auxiliando na gestão do cultivo."
    tags = "fluxo de caixa,financeiro,gestao"
    clienteId = 1
},
@{
    conteudo = "A produção pode ser organizada em ciclos, com variações de custos e receitas ao longo das fases do cultivo."
    tags = "ciclo producao,fases cultivo,planejamento"
    clienteId = 1
},
@{
    conteudo = "O aumento da biomassa e da alimentação ao longo do ciclo eleva os custos operacionais, principalmente com ração e energia."
    tags = "biomassa,custos crescentes,racao,energia"
    clienteId = 1
},
@{
    conteudo = "A conversão alimentar influencia diretamente o custo de produção e a eficiência do sistema."
    tags = "conversao alimentar,eficiencia,custo producao"
    clienteId = 1
},
@{
    conteudo = "A gestão eficiente dos custos é essencial para garantir margem de lucro em sistemas intensivos de aquicultura."
    tags = "gestao financeira,lucro,eficiencia"
    clienteId = 1
},
@{
    conteudo = "Despesas financeiras como juros e taxas bancárias impactam o resultado final do empreendimento."
    tags = "juros,despesas financeiras,custo total"
    clienteId = 1
},
@{
    conteudo = "Bonificações e incentivos a funcionários podem ser considerados dentro da estrutura de custos operacionais."
    tags = "bonus,funcionarios,custos operacionais"
    clienteId = 1
},
@{
    conteudo = "O planejamento financeiro deve considerar custos variáveis, custos fixos e projeção de receita para avaliar a sustentabilidade do cultivo."
    tags = "planejamento financeiro,viabilidade,projecao"
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