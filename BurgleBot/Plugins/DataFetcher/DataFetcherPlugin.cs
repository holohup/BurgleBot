using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace BurgleBot.Plugins.DataFetcher;

public sealed class DataFetcherPlugin
{
    [KernelFunction, Description("Provides a sample text to test the prompt")]
    public string GetSampleText() => 
        """
        Every summer, the town of Greenfield celebrates the Solstice Festival in Centennial Park. This year, twenty-five families gathered under a canopy of twinkling lanterns.
        	1.	The Smith, Johnson, Williams, Brown & Jones families
        Alice Smith welcomed her adventurous daughter Alice with a warm hug. Nearby, Bob Johnson and his introspective son Bob exchanged book recommendations, while Carol Williams guided her creative daughter Carol to the art stall. David Brown and his courageous son David tested their aim at the archery booth, and Emma Jones cheered on her optimistic daughter Emma as she raced in the three-legged contest.
        	2.	The Miller, Davis, Garcia, Rodriguez & Martinez families
        Frank Miller’s pragmatic advice helped witty Frank win the ring-toss prize. Grace Davis organized a flower-crown workshop alongside her empathetic daughter Grace, and Henry Garcia patiently taught curious Henry to fly a kite. Isabella Rodriguez and her determined daughter Isabella sang a duet on the small stage, and Jack Martinez and humorous Jack served as friendly judges for the pie-eating contest.
        	3.	The Hernandez, Lopez, Gonzalez, Wilson & Anderson families
        Katherine Hernandez applauded her diligent daughter Katherine in the spelling bee. Adventurous Liam Lopez led his son Liam on the obstacle course, while Mia Gonzalez coached her artistic daughter Mia in face-painting. Noah Wilson and reflective Noah strolled through the crafts market, and Olivia Anderson shared leadership tips with her charismatic daughter Olivia during the community-leaders meetup.
        	4.	The Thomas, Taylor, Moore, Jackson & Martin families
        Peter Thomas and methodical Peter built a towering block sculpture together. Quinn Taylor discussed strategy with analytical Quinn over chess on a park bench, as Rachel Moore cheered on compassionate Rachel in the storytelling circle. Samuel Jackson and bold Samuel teamed up for the tug-of-war, and Tina Martin rooted for inquisitive Tina in the science-demo tent.
        	5.	The Lee, Perez, Thompson, White & Harris families
        Ulysses Lee and determined Ulysses raced in the three-legged event, with Ulysses Lee’s experience giving them an edge. Victoria Perez and diplomatic Victoria consulted residents at the town-planning booth, while William Thompson and thoughtful William collaborated on the community mural. Xena White guided fearless Xena through the obstacle maze, and Yara Harris comforted her resilient daughter Yara after a near-spill on the balance beam.
        
        As the sun dipped below the horizon, lanterns were lit and families mingled across booths. Alice exchanged travel stories with Liam, Emma taught Mia a new dance step, and Bob helped Grace untangle her kite string. By nightfall, the twenty-five pairs had forged new friendships, laughed under the stars, and strengthened bonds that would last until next year’s Solstice Festival—when Greenfield would gather once more to celebrate family, friendship, and the enduring spirit of community.
        """;

    [KernelFunction, Description("Provides the names of persons which are expected to be extracted from the sample text")]
    public List<string> GetSampleTextExpectedNames() 
        => new List<string> { "Alice", "Bob", "Carol", "David", "Emma",
            "Frank", "Grace", "Henry", "Isabella", "Jack",
            "Katherine", "Liam", "Mia", "Noah", "Olivia",
            "Peter", "Quinn", "Rachel", "Samuel", "Tina",
            "Ulysses", "Victoria", "William", "Xena", "Yara",
            "Alice Smith", "Bob Johnson", "Carol Williams", "David Brown", "Emma Jones",
            "Frank Miller", "Grace Davis", "Henry Garcia", "Isabella Rodriguez", "Jack Martinez",
            "Katherine Hernandez", "Liam Lopez", "Mia Gonzalez", "Noah Wilson", "Olivia Anderson",
            "Peter Thomas", "Quinn Taylor", "Rachel Moore", "Samuel Jackson", "Tina Martin",
            "Ulysses Lee", "Victoria Perez", "William Thompson", "Xena White", "Yara Harris" };
}