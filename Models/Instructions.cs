namespace RecipeManager.Models
{
    public class Instruction
    {
        public int InstructionsID { get; set; }
        public int StepNumber { get; set; }
        public string InstructionText { get; set; }
        public int RecipeID { get; set; }
        public Recipe? Recipe { get; set; }
    }
}
