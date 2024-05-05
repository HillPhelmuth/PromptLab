using PromptLab.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PromptLab.Core.Models;

public class Prompt
{
    public const string PromptEngineerSystemPrompt =
  """
		Instruction: You are assisting a developer in creating and refining high-quality prompts for AI interaction. Use feedback from an evaluator agent and suggestions from a prompt expert agent to iteratively improve the prompts.
		
		## Example Workflow
		
		Developer's Initial Input: "Create a prompt for generating a news article summary."
		Your Initial Response: "Please specify the desired length of the summary and provide the text of the news article."
		Evaluator Agent's Feedback: "The prompt lacks specificity in the type of news article and the style of the summary."
		Prompt Expert's Suggestion: "Include details about the news article's subject and the desired tone of the summary."
		Your Follow-up: "Could you specify whether the news article is political, scientific, etc.? Also, would you prefer a bullet-point or a narrative summary style?"

		## Guidelines
		
		- Feedback Integration: Actively incorporate feedback from the evaluator agent to refine the prompt, focusing on clarity, specificity, and context.
		- Prompt Expert Integration: Actively request suggestions from the prompt expert agent to enhance the prompt's quality and effectiveness.
		- Clarity and Specificity Enhancements: Continuously ask for more specific details to narrow down the task requirements.
		- Contextual Information: Encourage the inclusion of context to better guide the AI’s response. This might include the subject area, desired tone, or specific examples of desired outcomes.
		- Iterative Refinement Process: Use a loop where you refine the prompt based on feedback from the evaluator agent until the desired quality and specificity are achieved.
		- Detailed Explanations: Provide explanations for each change suggested, helping the developer understand how each adjustment enhances the effectiveness of the prompt.
		- Multiple Iterations: Be prepared for several rounds of feedback and revisions to progressively refine the prompt.

		## System Behavior:
		
		- Maintain an adaptive and responsive approach, adjusting based on feedback from the evaluator agent.
		- Offer a structured format for each iteration to make the refinement process systematic and efficient.
		- Ensure transparency by clearly explaining the reasons for each modification or suggestion.
		- Ensure thoroughness by sharing every iteration of the prompt with the user before sending it for evaluation.
		""";

    public const string EvaluatorFunctionPrompt =
        """
		## Instruction
		
		You are tasked with evaluating prompts submitted by developers for their effectiveness in guiding AI interactions. Utilize your access to expert knowledge in prompt engineering to assess the clarity, specificity, contextuality, and overall quality of each prompt. Return a JSON object containing a numerical score and a detailed explanation of your assessment.

		## Best Practices in Prompt Engineering
		```
		{{ $guide }}
		```
		
		## Evaluation Criteria
		
		1. Clarity: Is the prompt clearly written? Does it communicate the task effectively without ambiguity?
		2. Specificity: Does the prompt provide specific details that narrow down the AI's scope of response?
		3. Contextual Relevance: Does the prompt include necessary context or background information that aids the AI in understanding the task?
		4. Alignment with Best Practices: Does the prompt adhere to established best practices in prompt engineering?
		5. Expected JSON Output Format:
		
		```json
		{
		"score": "<numerical_score_out_of_10>",
		"explanation": "<detailed_explanation_of_the_evaluation>"
		}
		```
		## Example Response
		
		Input Prompt from Developer: "Write a summary about this article."
		Evaluation Output:
		```json
		{
		"score": 4,
		"explanation": "The prompt lacks specificity regarding the desired length of the summary and the type of article. Including more detailed instructions about these aspects could improve the prompt's effectiveness."
		}
		```
		## System Behavior
		
		- Analyze each prompt against the evaluation criteria using your expert knowledge of best practices.
		- Generate a numerical score based on how well the prompt meets the criteria. The scale is 1 to 10, where 10 represents an ideal prompt.
		- Be an extremely critical, tough, and detail-oriented evaluator to provide valuable feedback to developers.
		- Provide a detailed explanation for the score, citing specific ways in which the prompt excels or falls short, and offering constructive suggestions for improvement.
		- If the score is is below 10, your explantion must include specific instructions for improving the prompt in a way that would increase the score by at least 1 point.

		Prompt to evaluate:
		{{ $prompt }}
		""";
    public static string PromptGuide => FileHelper.ExtractFromAssembly<string>("openaipromptguide.md");

    public const string PromptGuideTopics =
        """
		**Write clear instructions**

		These models can’t read your mind. If outputs are too long, ask for brief replies. If outputs are too simple, ask for expert-level writing. If you dislike the format, demonstrate the format you’d like to see. The less the model has to guess at what you want, the more likely you’ll get it. Tactics: - Include details in your query to get more relevant answers - Ask the model to adopt a persona - Use delimiters (quotes, xml, markdown) to clearly indicate distinct parts of the input - Specify the steps required to complete a task - Provide examples - Specify the desired length of the output

		**Provide reference text**

		Language models can confidently invent fake answers, especially when asked about esoteric topics or for citations and URLs. Providing reference text to these models can help in answering with fewer fabrications. Tactics: - Instruct the model to answer using a reference text - Instruct the model to answer with citations from a reference text

		**Split complex tasks into simpler subtasks**

		Decomposing a complex system into modular components is good practice in software engineering, and the same is true of tasks submitted to a language model. Complex tasks tend to have higher error rates than simpler tasks. Complex tasks can often be re-defined as a workflow of simpler tasks in which the outputs of earlier tasks are used to construct the inputs to later tasks. Tactics: - Use intent classification to identify the most relevant instructions for a user query - For dialogue applications that require very long conversations, summarize or filter previous dialogue - Summarize long documents piecewise and construct a full summary recursively

		**Give the model time to "think"**

		Models make more reasoning errors when trying to answer right away, rather than taking time to work out an answer. Asking for a "chain of thought" before an answer can help the model reason its way toward correct answers more reliably. Tactics: - Instruct the model to work out its own solution before rushing to a conclusion - Use inner monologue or a sequence of queries to hide the model's reasoning process - Ask the model if it missed anything on previous passes

		**Use external tools**

		Compensate for the weaknesses of the model by feeding it the outputs of other tools. For example, a text retrieval system can tell the model about relevant documents. A code execution engine like OpenAI's Code Interpreter can help the model do math and run code. If a task can be done more reliably or efficiently by a tool rather than by a language model, offload it to get the best of both. Tactics: - Use embeddings-based search to implement efficient knowledge retrieval - Use code execution to perform more accurate calculations or call external APIs - Give the model access to specific functions

		**Test changes systematically**

		Improving performance is easier if you can measure it. In some cases a modification to a prompt will achieve better performance on a few isolated examples but lead to worse overall performance on a more representative set of examples. Therefore to be sure that a change is net positive to performance it may be necessary to define a comprehensive test suite (also known an as an "eval"). Tactic: - Evaluate model outputs with reference to gold-standard answers
		""";
    public const string PromptModificationPrompt =
        $"""
		## Instruction
		Improve the prompts provided by a user using the Best Practices guide. Enhance the clarity, specificity, and contextuality of the prompts to guide the AI model more effectively. Think in a step-by-step way to provide the best possible prompt. Then, invoke 'SavePrompt' to save the prompt.
		Do not provide an explanation to the user. Keep that to yourself and respond with ONLY the improved prompt.

		## Best Practices guide
		{PromptGuideTopics}
		""";
}
public static class DefaultPrompts
{
    ////////////////////////////////////////////
    // Tree
    ////////////////////////////////////////////

    public static readonly PromptTemplate DefaultSummaryPrompt = new(
        """
        Write a summary of the following. Try to use only the information provided.
           Try to include as many key details as possible.
           
           {context_str}
           
           SUMMARY:"
        """,
        promptType: PromptType.Summary);

    public static readonly PromptTemplate DefaultInsertPrompt = new(
        """
        Context information is below. It is provided in a numbered list (1 to {num_chunks}), where each item in the list corresponds to a summary.
           ---------------------
           {context_list}
           ---------------------
           Given the context information, here is a new piece of information: {new_chunk_text}
           Answer with the number corresponding to the summary that should be updated. The answer should be the number corresponding to the summary that is most relevant to the question.
        """,
        promptType: PromptType.TreeInsert);

    public static readonly PromptTemplate DefaultQueryPrompt = new(
        """
        Some choices are given below. It is provided in a numbered list (1 to {num_chunks}), where each item in the list corresponds to a summary.
           ---------------------
           {context_list}
           ---------------------
           Using only the choices above and not prior knowledge, return the choice that is most relevant to the question: '{query_str}'
           Provide choice in the following format: 'ANSWER: <number>' and explain why this summary was selected in relation to the question.
        """,
        promptType: PromptType.TreeSelect);

    public static readonly PromptTemplate DefaultQueryPromptMultiple = new(
        """
        Some choices are given below. It is provided in a numbered list (1 to {num_chunks}), where each item in the list corresponds to a summary.
           ---------------------
           {context_list}
           ---------------------
           Using only the choices above and not prior knowledge, return the top choices (no more than {branching_factor}, ranked by most relevant to least) that are most relevant to the question: '{query_str}'
           Provide choices in the following format: 'ANSWER: <numbers>' and explain why these summaries were selected in relation to the question.
        """,
        promptType: PromptType.TreeSelectMultiple);

    public static readonly PromptTemplate DefaultRefinePrompt = new(
        """
        The original query is as follows: {query_str}
           We have provided an existing answer: {existing_answer}
           We have the opportunity to refine the existing answer (only if needed) with some more context below.
           ------------
           {context_msg}
           ------------
           Given the new context, refine the original answer to better answer the query. If the context isn't useful, return the original answer.
           Refined Answer:
        """,
        promptType: PromptType.Refine);

    public static readonly PromptTemplate DefaultTextQaPrompt = new(
        """
        Context information is below.
           ---------------------
           {context_str}
           ---------------------
           Given the context information and not prior knowledge, answer the query.
           Query: {query_str}
           Answer:
        """,
        promptType: PromptType.QuestionAnswer);

    public static readonly PromptTemplate DefaultTreeSummarizePrompt = new(
        """
        Context information from multiple sources is below.
           ---------------------
           {context_str}
           ---------------------
           Given the information from multiple sources and not prior knowledge, answer the query.
           Query: {query_str}
           Answer:
        """,
        promptType: PromptType.Summary);

    ////////////////////////////////////////////
    // Keyword Table
    ////////////////////////////////////////////

    public static readonly PromptTemplate DefaultKeywordExtractTemplate = new(
        """
        Some text is provided below. Given the text, extract up to {max_keywords} keywords from the text. Avoid stopwords.
           ---------------------
           {text}
           ---------------------
           Provide keywords in the following comma-separated format: 'KEYWORDS: <keywords>'
        """,
        promptType: PromptType.KeywordExtract);

    ////////////////////////////////////////////
    // Structured Store
    ////////////////////////////////////////////

    public static readonly PromptTemplate DefaultSchemaExtractPrompt = new(
        """
        We wish to extract relevant fields from an unstructured text chunk into a structured schema. We first provide the unstructured text, and then we provide the schema that we wish to extract.
           -----------text-----------
           {text}
           -----------schema-----------
           {schema}
           ---------------------
           Given the text and schema, extract the relevant fields from the text in the following format:
           field1: <value>
           field2: <value>
           ...
           If a field is not present in the text, don't include it in the output. If no fields are present in the text, return a blank string.
        """,
        promptType: PromptType.SchemaExtract);

    public static readonly PromptTemplate DefaultTextToSqlPrompt = new(
        """
        Given an input question, first create a syntactically correct {dialect} query to run, then look at the results of the query and return the answer. You can order the results by a relevant column to return the most interesting examples in the database.
           Never query for all the columns from a specific table, only ask for a few relevant columns given the question.
           Pay attention to use only the column names that you can see in the schema description. Be careful to not query for columns that do not exist. Pay attention to which column is in which table. Also, qualify column names with the table name when needed.
           You are required to use the following format, each taking one line:
           Question: Question here
           SQLQuery: SQL Query to run
           SQLResult: Result of the SQLQuery
           Answer: Final answer here
           Only use tables listed below.
           {schema}
           Question: {query_str}
           SQLQuery:
        """,
        promptType: PromptType.TextToSql);
    public static readonly PromptTemplate DefaultTextToSqlPgvectorPrompt = new(
        """
        Given an input question, first create a syntactically correct {dialect} query to run, then look at the results of the query and return the answer. You can order the results by a relevant column to return the most interesting examples in the database.

        Pay attention to use only the column names that you can see in the schema description. Be careful to not query for columns that do not exist. Pay attention to which column is in which table. Also, qualify column names with the table name when needed.

        IMPORTANT NOTE: you can use specialized pgvector syntax (`<->`) to do nearest neighbors/semantic search to a given vector from an embeddings column in the table. The embeddings value for a given row typically represents the semantic meaning of that row. The vector represents an embedding representation of the question, given below. Do NOT fill in the vector values directly, but rather specify a `[query_vector]` placeholder. For instance, some select statement examples below (the name of the embeddings column is `embedding`):
        SELECT * FROM items ORDER BY embedding <-> '[query_vector]' LIMIT 5;
        SELECT * FROM items WHERE id != 1 ORDER BY embedding <-> (SELECT embedding FROM items WHERE id = 1) LIMIT 5;
        SELECT * FROM items WHERE embedding <-> '[query_vector]' < 5;

        You are required to use the following format, each taking one line:

        Question: Question here
        SQLQuery: SQL Query to run
        """,
            promptType: PromptType.TextToSql);

    public static readonly PromptTemplate DefaultTableContextPrompt = new(
        """
        We have provided a table schema below.
        ---------------------
        {schema}
        ---------------------
        We have also provided context information below.
        {context_str}
        ---------------------
        Given the context information and the table schema,
        give a response to the following task: {query_str}
        """,
        promptType: PromptType.TableContext);

    public static readonly PromptTemplate DefaultRefineTableContextPrompt = new(
        """
        We have provided a table schema below.
        ---------------------
        {schema}
        ---------------------
        We have also provided some context information below.
        {context_msg}
        ---------------------
        Given the context information and the table schema,
        give a response to the following task: {query_str}
        We have provided an existing answer: {existing_answer}
        Given the new context, refine the original answer to better
        answer the question.
        If the context isn't useful, return the original answer.
        """,
        promptType: PromptType.TableContext);

    public static readonly PromptTemplate DefaultKgTripletExtractPrompt = new(
        """
        Some text is provided below. Given the text, extract up to {max_knowledge_triplets} knowledge triplets in the form of (subject, predicate, object). Avoid stopwords.
        ---------------------
        Example:
        Text: Alice is Bob's mother.
        Triplets:
        (Alice, is mother of, Bob)
        Text: Philz is a coffee shop founded in Berkeley in 1982.
        Triplets:
        (Philz, is, coffee shop)
        (Philz, founded in, Berkeley)
        (Philz, founded in, 1982)
        ---------------------
        Text: {text}
        Triplets:
        """,
        promptType: PromptType.KnowledgeTripletExtract);

    public static readonly PromptTemplate DefaultHydePrompt = new(
        """
        Please write a passage to answer the question
        Try to include as many key details as possible.

        {context_str}

        Passage:"
        """,
        promptType: PromptType.Hyde);

    public static readonly PromptTemplate DefaultSimpleInputPrompt = new(
"{query_str}",
        promptType: PromptType.SimpleInput);

    public static readonly PromptTemplate DefaultJsonPathPrompt = new(
        """
        We have provided a JSON schema below:
        {schema}
        Given a task, respond with a JSON Path query that can retrieve data from a JSON value that matches the schema.
        Provide the JSON Path query in the following format: 'JSONPath: <JSONPath>'
        You must include the value 'JSONPath:' before the provided JSON Path query.
        Example Format:
        Task: What is John's age?
        Response: JSONPath: $.John.age
        Let's try this now:

        Task: {query_str}
        Response:
        """,
        promptType: PromptType.JsonPath);

    public static readonly PromptTemplate DefaultChoiceSelectPrompt = new(
        """
        A list of documents is shown below. Each document has a number next to it along with a summary of the document. A question is also provided.
        Respond with the numbers of the documents you should consult to answer the question, in order of relevance, as well as the relevance score. The relevance score is a number from 1-10 based on how relevant you think the document is to the question.
        Do not include any documents that are not relevant to the question.
        Example format:
        Document 1:
        <summary of document 1>

        Document 2:
        <summary of document 2>

        ...
        ...
        Document 10:
        <summary of document 10>

        Question: <question>
        Answer:
        Doc: 9, Relevance: 7
        Doc: 3, Relevance: 4
        Doc: 7, Relevance: 3

        Let's try this now:

        {context_str}
        Question: {query_str}
        Answer:
        """,
        promptType: PromptType.ChoiceSelect);
    public static List<PromptTemplate> RagPromptTemplates = [DefaultSummaryPrompt, DefaultHydePrompt, DefaultKgTripletExtractPrompt, DefaultKeywordExtractTemplate];
}
public enum PromptType
{
    Summary,
    TreeInsert,
    TreeSelect,
    TreeSelectMultiple,
    Refine,
    QuestionAnswer,
    KeywordExtract,
    SchemaExtract,
    TextToSql,
    JsonPath, ChoiceSelect, TableContext, KnowledgeTripletExtract, SimpleInput, Hyde
}

public class PromptTemplate
{
    public string Template { get; }
    public PromptType PromptType { get; }

    public List<string> VariableNames
    {
        get
        {
            string input = Template;
            string pattern = @"\{([^{}]+)\}";
            var matches = Regex.Matches(input, pattern);
            List<string> names = new List<string>();

            foreach (Match match in matches)
            {
                names.Add(match.Groups[1].Value);
            }

            return names;
        }
    }

    public PromptTemplate(string template, PromptType promptType)
    {
        Template = template;
        PromptType = promptType;
    }
    public override string ToString()
    {
        var input = Template;
        const string pattern = @"\{([^\{\}]+)\}";
        const string replacement = "{{$${1}}}";
        return Regex.Replace(input, pattern, replacement);
    }
}