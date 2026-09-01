using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class CrossoverService
{
    private readonly GenomeBuilderService _genomeBuilder;

    private readonly ILogger<CrossoverService> _logger;

    public CrossoverService(
        GenomeBuilderService genomeBuilder,
        ILogger<CrossoverService> logger)
    {
        _genomeBuilder = genomeBuilder;
        _logger = logger;
    }

    public async Task<Dictionary<string, decimal>> CreateGenomeAsync(
        string parentA,
        string parentB)
    {
        var genomeA =
            await _genomeBuilder
                .BuildAsync(parentA);

        var genomeB =
            await _genomeBuilder
                .BuildAsync(parentB);

        _logger.LogInformation(
            """
            Parent A Genome
            
            Strategy={strategy}
            
            Parameters={count}
            
            """,
            parentA,
            genomeA.Parameters.Count);
        
        _logger.LogInformation(
            """
            Parent B Genome
            
            Strategy={strategy}
            
            Parameters={count}
            
            """,
            parentB,
            genomeB.Parameters.Count);

        _logger.LogInformation(
            "GenomeA Keys={keys}",
            string.Join(", ",
                genomeA.Parameters.Keys));
        
        _logger.LogInformation(
            "GenomeB Keys={keys}",
            string.Join(", ",
                genomeB.Parameters.Keys));

        // ==========================================
        // COMBINE PARAMETER NAMES 
        // ==========================================
        //
        // Use union so that a parameter existing
        // in either parent can be inherited by the child.
        //

        var parameters =
            genomeA.Parameters.Keys
                .Union(genomeB.Parameters.Keys)
                .OrderBy(x => x)
                .ToList();

        if (parameters.Count == 0)
        {
            _logger.LogWarning(
                """
                Genome crossover skipped
                
                ParentA={parentA}
                
                ParentB={parentB}
                
                Reason=Both genomes contain zero parameters
                
                """,
                parentA,
                parentB);
            
            return new Dictionary<string, decimal>();
        
        }

        var valuesA =
            new Dictionary<string, decimal>();
        
        var valuesB =
            new Dictionary<string, decimal>();

        foreach (var parameter in parameters)
        {
            if (genomeA.Parameters.TryGetValue(
                parameter,
                out var valueA))
            {
                valuesA[parameter] =
                    valueA;
            }
            else if (genomeB.Parameters.TryGetValue(
                parameter, out var fallbackA))
            {
                valuesA[parameter] =
                    fallbackA;
            }
            
            if (genomeB.Parameters.TryGetValue(
                parameter, out var valueB))
            {
                valuesB[parameter] =
                    valueB;
            }
            else if (genomeA.Parameters.TryGetValue(
                parameter, out var fallbackB))
            {
                valuesB[parameter] =
                    fallbackB;
            }
        }

        // ==========================================
        // MULTI-POINT CROSSOVER
        // ==========================================

        int count =
            parameters.Count;

        var child =
            new Dictionary<string, decimal>();


        // With fewer than 3 parameters, two crossover
        // points are not meaningful.
        if (count < 3)
        {
            for (int i = 0; i < count; i++)
            {
                string parameter =
                    parameters[i];

                child[parameter] =
                    Random.Shared.Next(2) == 0
                        ? valuesA[parameter]
                        : valuesB[parameter];
            }
            
            _logger.LogInformation(
                """
                Genome crossover
                
                Mode=Simple
                
                ParentA={parentA}
                
                ParentB={parentB}
                
                Parameters={count}
                
                """,
                parentA,
                parentB,
                count);
            
            return child;
        }

        // ==========================================
        // SELECT TWO CROSSOVER POINTS
        // ==========================================
        
        int point1 =
            Random.Shared.Next(
                1,
                count - 1);
        
        int point2 =
            Random.Shared.Next(
                1,
                count - 1);
        
        while (point2 == point1)
        {
            point2 =
                Random.Shared.Next(
                    1,
                    count - 1);
        
        }
        
        if (point1 > point2)
        {
            (point1, point2) =
                (point2, point1);
        }

        // ==========================================
        // RANDOM START PARENT
        // ==========================================
        
        bool useParentA =
            Random.Shared.Next(2) == 0;


        // ==========================================
        // BUILD CHILD
        // ==========================================

        for (int i = 0; i < count; i++)
        {
            string parameter =
                parameters[i];
            
            // Switch parent at each crossover point.
            if (i == point1 ||
                i == point2)
            {
                useParentA =
                    !useParentA;
            }
            
            child[parameter] =
                useParentA
                    ? valuesA[parameter]
                    : valuesB[parameter];
        }

        _logger.LogInformation(
            """
            Genome multi-point crossover
            
            ParentA={parentA}
            
            ParentB={parentB}
            
            Parameters={count}
            
            CrossoverPoint1={point1}
            
            CrossoverPoint2={point2}
            
            StartingParent={startingParent}

            """,
            parentA,
            parentB,
            count,
            point1,
            point2,
            useParentA
                ? "A"
                : "B");
        
        return child;

    }
}
