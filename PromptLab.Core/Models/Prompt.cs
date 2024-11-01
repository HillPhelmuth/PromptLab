using PromptLab.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptLab.Core.Models;

public class Prompt
{
	public const string PromptEngineerSystemPrompt =
  """
		Instruction: You are assisting a developer in creating and refining high-quality prompts for AI interaction. Use feedback from an evaluator agent and suggestions from a prompt expert agent to iteratively improve the prompts.
		
		## Guidelines
		
		- Feedback Integration: Actively incorporate feedback from the evaluator agent to refine the prompt, focusing on clarity, specificity, and context.
		- Prompt Expert Integration: Actively request suggestions from the prompt expert agent to enhance the prompt's quality and effectiveness.
		- Clarity and Specificity Enhancements: Continuously ask for more specific details to narrow down the task requirements.
		- Contextual Information: Encourage the inclusion of context to better guide the AI’s response. This might include the subject area, desired tone, or specific examples of desired outcomes.
		
		- Detailed Explanations: Provide explanations for each change suggested, helping the developer understand how each adjustment enhances the effectiveness of the prompt.
		

		## System Behavior:
		
		- Maintain an adaptive and responsive approach, adjusting based on feedback from the evaluator agent.
		- Offer a structured format for each iteration to make the refinement process systematic and efficient.
		- Ensure transparency by clearly explaining the reasons for each modification or suggestion.
		- Ensure thoroughness by sharing every iteration of the prompt with the user before sending it for evaluation.
		""";

	public const string EvaluatorFunctionPrompt =
		$$$"""
		## Instruction
		
		You are tasked with evaluating prompts submitted by developers for their effectiveness in guiding AI interactions. Utilize your access to the Prompt Engineering Guide to assess the clarity, specificity, contextuality, and overall quality of each prompt. Return a JSON object containing a numerical score, a detailed explanation of your assessment, and suggestions for improvement.
		NOTE: Unless otherwised specified by the user, any prompt submitted for evaluation should be assumed to be a 'system instructions' prompt or 'meta-prompt', meaning the developer provides instructions to guide the AI model.

		## Prompt Engineering Guide


		### Best Practices in Prompt Engineering
		```
		{{{PromptGuideTopics}}}
		```
		
		### Tips to Become a Better Prompt Engineer

		```
		{{{PromptTips}}}
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
		"explanation": "<detailed_explanation_of_the_evaluation>",
		"tipsForImprovement": "<suggestions_for_improving_the_prompt>"
		}
		```
		## System Behavior
		
		- Analyze each prompt against the evaluation criteria using your expert knowledge of best practices.
		- Generate a numerical score based on how well the prompt meets the criteria. The scale is 1 to 10, where 10 represents an ideal prompt.
		- Be an extremely critical, tough, and detail-oriented evaluator to provide valuable feedback to developers.
		- Provide a detailed explanation for the score, citing specific ways in which the prompt excels or falls short, and offering constructive suggestions for improvement.
		- If the score is is below 10, your explantion must include specific instructions for improving the prompt in a way that would increase the score by at least 1 point.

		Prompt to evaluate:
		```markdown
		{{ $prompt }}
		```
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
	public const string PromptTips = 
		"""
		 ## 15 Tips to Become a Better Prompt Engineer for Generative AI
		
		Do you want to maximize the potential of your favorite large language models? Prompt engineering is a critical skill for building intelligent apps with generative AI. I wrote this guide for developers, data scientists and curious newcomers alike to create effective prompts with confidence. Below, I share 15 essential tips, and feel free to comment with your own.
		
		**1. Understand the Basics**
		
		Text prompts are a fundamental way for users to interact with generative language models, which aim to generate the next sequence of words that are most likely to follow from the given input text. To put it simply, it's like asking the model, "What comes to your mind when I say &lt;prompt&gt;?" The following example demonstrate this behavior effectively. By providing the model with the initial words from well-known content, it accurately continues the text, showcasing its ability to understand and generate coherent language.
		
		| **Prompt** | **Completion** |
		| --- | --- |
		| To be or not to be | , that is the question […] |
		
		**2. Identify Prompt Components**
		
		When constructing a prompt, it can be helpful to identify its underlying components. These components include instructions, primary content, examples, cues, and supporting content. Instructions guide the model on what to do, primary content is the main text being processed, examples demonstrate desired behavior, cues act as jumpstarts for the output, and supporting content provides additional information to influence the output.
		
		**3. Craft Clear Instructions**
		
		Instructions are commonly used in prompts and guide the model's behavior. Be specific and leave as little room for interpretation as possible. Use analogies and descriptive language to help the model understand your desired outcome.
		
		| **Instruction Type** | **Prompt** | **Completion** |
		| --- | --- | --- |
		| Simple instructions | Write a brief product description for our new cloud-based collaboration tool | Introducing ContosoSync<br>ContosoSync is a cutting-edge cloud-based collaboration tool designed to streamline teamwork and boost productivity for businesses of all sizes. With real-time document editing, seamless file sharing, and integrated task management, ContosoSync empowers teams to work together effortlessly, whether they are in the office or spread across the globe. |
		| Complex Instructions | Write a compelling email inviting potential clients to our upcoming product launch event for ContosoSync. Include benefits of attending and express gratitude of support. | Subject: Join Us for an Exclusive Product Launch Event for ContosoSync!<br><br>Dear [Client's Name],<br><br>We are thrilled to invite you to our exclusive ContosoSync product launch event on [Date]! As a valued industry leader, we believe you'll find our latest innovation both exciting and game-changing.<br><br>Event Details:<br>Date: [Date]<br>Time: [Time]<br>Location: [Venue]<br><br>During the event, you'll have the opportunity to:<br><br>1. Be the first to witness our groundbreaking product in action.<br>2. Interact with our team of experts and get your questions answered.<br>3. Network with like-minded professionals from various industries.<br><br>This launch marks a significant milestone for our company, and we'd be honored to have your esteemed presence at the event. Please RSVP by [RSVP Date] to ensure your spot.<br><br>To confirm your attendance or for any inquiries, kindly reply to this email or reach out to [Contact Person/Email/Phone].<br><br>We look forward to sharing this exciting moment with you and exploring potential collaboration opportunities.<br><br>Best regards,<br>[Your Name]<br>[Your Title/Position]<br>[Your Company Name] |
		
		**4. Utilize Examples**
		
		Examples play a vital role in prompt engineering. By including input and output pairs, you condition the model to respond as desired for the current inference. One-shot or few-shot learning techniques, where you provide examples of the desired behavior, can significantly improve the model's output accuracy.
		
		| **Learning Type** | **Prompt** | **Completion** |
		| --- | --- | --- |
		| Zero-shot | Headline: Coach confident injury won't derail Warriors<br>Topic: | The coach is confident that the injury won't derail the Warriors' season. The team is still focused on their goals and that they will continue to work hard to achieve them. |
		| Few-shot | Headline: Twins' Correa to use opt-out, test free agency<br>Topic: Baseball<br>Headline: Qatar World Cup to have zones for sobering up<br>Topic: Soccer<br>Headline: Yates: Fantasy football intel for Week 6<br>Topic: Football<br>Headline: Coach confident injury won't derail Warriors<br>Topic: | Basketball |
		
		**5. Pay Attention to Cueing**
		
		Cues provide a starting point for the model's output. They help direct the model towards the desired response. Including cues in your prompt can help guide the model to generate output aligned with your intentions.
		
		| **Number of Cues** | **Prompt** | **Completion** |
		| --- | --- | --- |
		| 0 | Performance reviews are a core component of our Performance & Development approach at Contoso. […]<br>Summarize the above email message: | The purpose of this email is to inform employees of recent changes to the performance review experience, which is part of Contoso's Performance & Development approach. The changes include an improved structure, upgraded in-tool user experience, and focused prompts. Additionally, there is now an optional Conversation Starters section for employees to flag topics they'd like to discuss with their managers. |
		| 1 | Performance reviews are a core component of our Performance & Development approach at Contoso. […]<br>Summarize the above email message:<br>The key takeaway of the message is | that Contoso is improving the performance review experience by making it more focused on core priorities, expectations, and impact. Additionally, they are upgrading the in-tool user experience with tips, resources, and guidance. Finally, they are adding guidance on how to activate company-wide core priorities. |
		| 2 | Performance reviews are a core component of our Performance & Development approach at Contoso. […]<br>Summarize the above email message:<br>Key Points:<br>• | Contoso is introducing changes to the performance review experience, based on feedback from employees.<br>• The changes include a new focus on core priorities, expectations, and impact; an upgraded user experience; and new guidance on company-wide core priorities.<br>• Performance reviews are a time to reflect on past performance and learning as well as to plan for future priorities and growth opportunities.<br>• Employees and managers should write comments in advance of the performance review meeting.<br>• There should be at least two performance reviews per year.<br>• The time investment for each review should remain about the same. |
		
		**6. Test Arrangements**
		
		The order in which information is presented in the prompt can impact the model's output. Experiment with different arrangements of instructions, primary content, examples, and cues to achieve the desired results. Recency bias, where the model is influenced more by recent information, is something to keep in mind.
		
		**7. Give the Model an "Out"**
		
		Sometimes, the model may struggle to complete a task accurately. To mitigate this, provide alternative paths or instructions for the model to follow if it cannot find a satisfactory response. This helps prevent the generation of false or nonsensical answers.
		
		| **System message** | **User** | **Assistant** |
		| --- | --- | --- |
		| You are an AI assistant that helps people find information. Answer in as few words as possible. | John Smith is married to Lucy Smith. They have five kids, and he works as a software engineer at Microsoft. What search queries should I do to fact-check this? ## One possible search query is: | "John Smith married Lucy Smith five kids software engineer Microsoft" |
		
		**8. Be Mindful of Token Limits**
		
		Prompt size is limited by the number of tokens the model can handle. Tokens represent individual units of text. Use space efficiently by considering factors like table formatting for data inclusion and optimizing whitespace usage. For example, The Azure OpenAI text-davinci-003 model has a limit of 4097 tokens of combined input and output. Given that we want around 250 words of output and a token represents about one-half of a word, we will reserve 500 tokens for the completion response. That leaves ~3500 tokens for input, or about 1700 words. Any source text longer than 1700 words will need to be manually edited to fit.
		
		(Image Replacement text)
		
		The image is a visualization of text tokenization, which highlights how a sentence is split into individual tokens. The text "Recent work has demonstrated substantial gains on many NLP tasks and benchmarks by pre-training on a large corpus of text followed by fine-tuning on a specific task. While typically task-agnostic in architecture, this method still requires task-specific fine-tuning datasets of thousands or tens of thousands of examples." is color-coded to display individual tokens. Beside this, various date formats like "October 18th 2022", "October 18 2022", "2022/10/18", and "10-18-2022" are tokenized differently, showing how slight changes in formatting result in different token counts.
		
		**10. Experiment and Iterate**
		
		Prompt engineering is an art that requires experimentation and iteration. Practice and gain experience in crafting prompts for different tasks. Every model may behave differently, so it's important to adapt your prompt engineering techniques accordingly.
		
		**Advanced Tips**
		Ready to go deeper? Here are 5 advanced tips for prompt engineering with Azure OpenAI Service.
		
		**11. Understand the Different APIs**
		
		Depending on the model you're using, such as GPT-35-Turbo or GPT-4, you'll need to format your input differently for the Chat Completion API or the Completion API. [Familiarize yourself with the specific requirements of each API](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models) to ensure proper prompt design.
		
		**12. Start with a Clear System Message**
		
		The system message sets the context and instructions for the model. Describe the assistant's role, limitations, and desired response format. Clearly define what the model should and shouldn't answer, and provide guidelines for generating accurate and helpful responses.
		
		| **System message** | **User** | **Assistant** |
		| --- | --- | --- |
		| You are an AI assistant that helps people find information and responds in rhyme. If the user asks you a question you don't know the answer to, say so. | What can you tell about me, Clippy? | Dear Clippy, I'm sorry to say,<br>But I don't have info on you today.<br>I'm just an AI with knowledge in my brain,<br>But without your input, I can't explain.<br>So please tell me more about what you seek,<br>And I'll do my best to give you an answer unique. |
		
		**13. Leverage Few-Shot Learning**
		
		Few-shot learning involves providing a set of training examples as part of the prompt to give additional context to the model. For the Chat Completions API, use a series of messages between the user and assistant as examples to prime the model's behavior and seed answers to common questions.
		
		| **System message** | **Few-shot examples** |
		| --- | --- |
		| Assistant is an intelligent chatbot designed to help users answer their tax related questions.<br>Instructions<br>If the user asks when they should file their taxes, remind them the deadline is April 15th. | User: When should I file my taxes?<br>Assistant: You should file your taxes by April 15th. |
		
		**14. Use Explicit Constraints**
		
		Explicit constraints help guide the model's behavior and responses. For example, you can specify that the model must answer in a specific format, such as a poem or a tweet, to align with your application requirements.
		
		| **System message** | **User** | **Assistant** |
		| --- | --- | --- |
		| Assistant is an AI trained to provide short, tweet-like responses. Respond accordingly. | What's the capital of France? | Paris, the city of romance and lights. |
		
		**15. Combine Techniques for Complex Tasks**
		
		For more complex tasks, you can combine various techniques such as few-shot learning, explicit constraints, and explicit instructions to guide the model's behavior effectively. Experiment and iterate to find the optimal combination for your use case.
		""";

    public const string UserPromptGuide = """
                                          # ULTIMATE CHATGPT PROMPT GUIDE FOR 2024
                                          AI WILL NOT TAKE OVER.  
                                          BUT HUMANS WHO CAN FULLY UTILISE AI WILL.  
                                          (Save this now)

                                          ---

                                          ## Good Prompts vs Bad Prompts

                                          ### Technology Inquiry:
                                          - **Bad Prompt:** Tell me about EV?
                                          - **Good Prompt:** What are some breakthroughs in electric car technology in the last two years?

                                          ### Business Insights:
                                          - **Bad Prompt:** How do I increase sales?
                                          - **Good Prompt:** What are some proven market entry strategies for a new tech product in a competitive market?

                                          ### Historical Analysis:
                                          - **Bad Prompt:** Tell me about Hitler.
                                          - **Good Prompt:** Could you analyze the economic impact of the Treaty of Versailles following World War II?

                                          ---

                                          ## Best Practices to Use ChatGPT
                                          - **Be clear and specific:** Clarity improves the response of queries.
                                          - **Provide Context:** Give scenarios or examples for informed output.
                                          - **Use Previous Answers:** Incorporate recent outputs.
                                          - **Use Explicit Instructions:** Define clear boundaries.
                                          - **Refine and Revise:** Adapt questions, aim for concise responses.
                                          - **Start with Instructions:** Tell the model you want it to behave.
                                          - **Use Complete Sentences:** Frame questions for more detailed answers.
                                          - **Experiment and Iterate:** Make changes and refine prompts.
                                          - **Limit the Response Length:** Ask for summaries or brief reports.
                                          - **Verify and Re-check Info:** Use for informational queries.

                                          ---

                                          ## Prompts for Content Creation
                                          - Write an introduction for a blog post on [topic].
                                          - Develop a step-by-step tutorial on [topic] for a YouTube channel.
                                          - Create a list of [10] writing prompts related to [topic]. Includes keywords.
                                          - Compile a list of trending content ideas for an online blog on [topic].
                                          - Develop a newspaper op-ed examining [number] of words.
                                          - Write a detailed email to a friend expanding on [topic].
                                          - Rewrite this paragraph in a bulleted list form.

                                          ## Prompts for Sales
                                          - Develop a compelling sales pitch for [product/service], using [information].
                                          - Write a follow-up email to a lead who has shown interest in [product/service].
                                          - Create a personalized email for a marketing campaign for [target audience].
                                          - Compose a personalized response to a cold outreach email from [recipient].
                                          - Generate a list of [10] objection-handling statements on [concerns].
                                          - Write a cold email sequence for reaching potential partners.
                                          - Develop a retention guide on [intersection of business sectors].

                                          ## Prompts for Marketing
                                          - Create a LinkedIn post under 150 words using this [insert copy here].
                                          - Write persuasive copy aimed at convincing [target group].
                                          - List 10 calls to action (CTAs) for a landing page promoting [product].
                                          - Write a campaign outline for boosting a workshop based on [event].
                                          - Write a [number] CTA reasons to get [product/service].
                                          - Using [framework] create a short product user-based journey.

                                          ## Prompts for Data Science
                                          - Act as a data scientist and build a machine learning model for [task].
                                          - Write a step-by-step guide on feature engineering for [dataset].
                                          - Explain model evaluation and interpretation with [model].
                                          - Walk through feature scaling techniques and steps.
                                          - Build a random forest model using [framework]. Link the steps.
                                          - Conduct exploratory data analysis on [dataset].
                                          - Gather 3 cases of implementations of unsupervised clustering.

                                          ## Prompts for Web Development
                                          - List [number] UI development principles suited for a landing page.
                                          - Create a HTML form that contains [number] input text.
                                          - Generate CSS rules for a responsive layout for a website design.
                                          - Explain the impact of [framework/library] in web development.

                                          ## Prompts for Customer Success
                                          - Develop a step-by-step guide on handling complaints.
                                          - Provide a response for an unhappy customer with [issue].
                                          - Use the RATER model principles on improving customer experiences.
                                          - Create an empathy-driven email to engage [recipient].
                                          - Draft a communication plan for a new feature to integrate with [service].

                                          ---  

                                          ## Social Media Contact Information
                                          - Follow for more on: (Icons for Social media platforms: Medium, Inst,^G, LinkedIn)

                                          ---
                                          """;

    public const string MasterChatGptPromptGuide = 
                """
                # Master ChatGPT Prompt Guide

                ---

                ## Temperature in Prompting ChatGPT

                | **Parameter**         | **Description**                                               | **Effect**                                                                  | **Example**                                                                 |
                |-----------------------|---------------------------------------------------------------|-----------------------------------------------------------------------------|------------------------------------------------------------------------------|
                | High Temperature (e.g. 0.8 to 1.0) | The AI’s responses are more diverse and creative              | The AI may produce unexpected or more random responses                      | If asked to continue the story about a princess, the AI might introduce aliens or time travel  |
                | Medium Temperature (e.g. 0.5 to 0.7) | The AI’s responses are balanced between randomness and focus  | The AI might show moderate creativity, with responses that fit the context but include some surprising elements                    | If asked to continue the story about a princess, the AI might introduce a talking animal companion  |
                | Low Temperature (e.g. 0.1 to 0.4) | The AI’s responses are more deterministic and focused         | The AI will stick closely to the most expected response, with little creativity | If asked to continue the story about a princess, the AI might stick with traditional fairy tale elements like a prince or a witch |

                ---

                ## Tones in Prompting ChatGPT

                | **Tone**        | **Description**                                  | **Example Prompt**                                         |
                |-----------------|--------------------------------------------------|------------------------------------------------------------|
                | Friendly        | Conversational and warm                          | As a friendly AI, tell me a story about a dog.             |
                | Formal          | Professional, polite, and respectful             | As a formal AI, write a business proposal.                 |
                | Casual          | Informal, using colloquial language              | As a casual AI, tell me how to make a sandwich.            |
                | Professional    | Focused, clear, and businesslike                 | As a professional AI, explain the blockchain technology.   |
                | Humorous        | Funny and entertaining                           | As a humorous AI, tell me a joke.                          |
                | Sincere         | Honest and heartfelt                             | As a sincere AI, tell me what you think about art.         |
                | Excited         | Energetic and enthusiastic                       | As an excited AI, describe a roller coaster ride.          |
                | Encouraging     | Positive and supportive                          | As an encouraging AI, motivate me to exercise.             |
                | Respectful      | Polite and showing deference                     | As a respectful AI, explain cultural customs in Japan.     |
                | Enthusiastic    | Eager and spirited                               | As an enthusiastic AI, describe the future of space travel.|
                | Serious         | Solemn and not joking                            | As a serious AI, discuss the implications of climate change.|
                | Sarcastic       | Ironic and satirical                             | As a sarcastically toned AI, tell me about the joys of rush hour traffic. |
                | Sympathetic     | Compassionate and understanding                  | As a sympathetic AI, respond to a sad personal story.      |

                ---

                ## ChatGPT Ultimate Prompting Guide

                - **Tone**: Specify the desired tone (e.g. formal, casual, informative, persuasive).
                - **Formal**: Define the format or structure (e.g. essay, bullet points, outline, dialogue).
                - **Act as**: Indicate a role or perspective to adopt (e.g. expert, critic, enthusiast).
                - **Objective**: State the goal or purpose of the response (e.g. inform, persuade, entertain).
                - **Context**: Provide background information, data, or context for accurate content generation.
                - **Scope**: Define the scope or range of the topic.
                - **Keywords**: List important keywords or phrases to be included.
                - **Limitations**: Specify constraints, such as word or character count.
                - **Examples**: Provide examples of desired style, structure, or content.
                - **Deadline**: Mention deadlines or time frames for time-sensitive responses.
                - **Audience**: Specify the target audience for tailored content.
                - **Language**: Indicate the language for the response, if different from the prompt.
                - **Citations**: Request inclusion of citations or sources to support information.
                - **Points of view**: Ask the AI to consider multiple perspectives or opinions.
                - **Counterarguments**: Request addressing potential counterarguments.
                - **Terminology**: Specify industry-specific or technical terms to use or avoid.
                - **Analogies**: Ask the AI to use analogies or examples to clarify concepts.
                - **Quotes**: Request inclusion of relevant quotes or statements from experts.
                - **Statistics**: Encourage the use of statistics or data to support claims.
                - **Visual elements**: Inquire about including charts, graphs, or images.
                - **Call to action**: Request a clear call to action or next steps.
                - **Sensitivity**: Mention sensitive topics or issues to be handled with care or avoided.

                ---
                """;
}