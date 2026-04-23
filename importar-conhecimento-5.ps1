$baseUrl = "http://localhost:5205/conhecimento"

$dados = @(

@{
    conteudo = "A análise de cenários permite avaliar diferentes resultados financeiros a partir de variações nos parâmetros de produção."
    tags = "cenarios,analise,projecao,planejamento"
    clienteId = 1
},
@{
    conteudo = "Mudanças no preço de venda impactam diretamente a receita e a lucratividade do cultivo."
    tags = "preco venda,receita,lucro,mercado"
    clienteId = 1
},
@{
    conteudo = "O aumento no custo da ração reduz a margem de lucro e pode comprometer a viabilidade econômica."
    tags = "racao,custo,impacto lucro"
    clienteId = 1
},
@{
    conteudo = "A taxa de sobrevivência dos animais é um dos principais fatores que influenciam o resultado financeiro."
    tags = "sobrevivencia,producao,resultado financeiro"
    clienteId = 1
},
@{
    conteudo = "A conversão alimentar impacta diretamente o consumo de ração e o custo total de produção."
    tags = "conversao alimentar,custo,eficiencia"
    clienteId = 1
},
@{
    conteudo = "O aumento da densidade de estocagem pode elevar a produção total, mas também aumenta riscos e custos operacionais."
    tags = "densidade,risco,producao,custo"
    clienteId = 1
},
@{
    conteudo = "A variação no custo de energia afeta significativamente sistemas intensivos com alta dependência de aeração."
    tags = "energia,custo,aeracao,impacto"
    clienteId = 1
},
@{
    conteudo = "Cenários pessimistas consideram queda de preço, aumento de custos e menor desempenho produtivo."
    tags = "cenario pessimista,risco,queda preco"
    clienteId = 1
},
@{
    conteudo = "Cenários otimistas consideram melhor preço de venda, alta sobrevivência e boa conversão alimentar."
    tags = "cenario otimista,alta performance,lucro"
    clienteId = 1
},
@{
    conteudo = "A análise de sensibilidade identifica quais variáveis têm maior impacto no resultado financeiro."
    tags = "sensibilidade,variaveis criticas,analise"
    clienteId = 1
},
@{
    conteudo = "O ponto de equilíbrio representa o nível mínimo de produção ou preço necessário para cobrir os custos."
    tags = "ponto equilibrio,break even,financeiro"
    clienteId = 1
},
@{
    conteudo = "Pequenas variações em parâmetros-chave podem gerar grandes impactos no lucro final do cultivo."
    tags = "variacao impacto,lucro,risco"
    clienteId = 1
},
@{
    conteudo = "A diversificação de estratégias reduz o risco financeiro em sistemas de produção aquícola."
    tags = "diversificacao,risco,estrategia"
    clienteId = 1
},
@{
    conteudo = "O planejamento baseado em cenários melhora a tomada de decisão e reduz incertezas no cultivo."
    tags = "planejamento,decisao,gestao"
    clienteId = 1
},
@{
    conteudo = "A margem de lucro deve ser analisada em diferentes cenários para garantir sustentabilidade do negócio."
    tags = "margem lucro,sustentabilidade,analise"
    clienteId = 1
},
@{
    conteudo = "Custos fixos mantêm-se constantes mesmo com variação de produção, impactando mais cenários de baixa produtividade."
    tags = "custos fixos,producao baixa,impacto"
    clienteId = 1
},
@{
    conteudo = "Custos variáveis aumentam conforme a produção, sendo diretamente proporcionais ao volume cultivado."
    tags = "custos variaveis,producao,crescimento"
    clienteId = 1
},
@{
    conteudo = "A eficiência produtiva é determinante para manter rentabilidade em cenários adversos."
    tags = "eficiencia,producao,rentabilidade"
    clienteId = 1
},
@{
    conteudo = "Simulações permitem testar estratégias antes da implementação real no sistema produtivo."
    tags = "simulacao,teste estrategia,planejamento"
    clienteId = 1
},
@{
    conteudo = "A gestão baseada em dados melhora a previsibilidade e o controle financeiro da produção aquícola."
    tags = "dados,gestao,previsibilidade"
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