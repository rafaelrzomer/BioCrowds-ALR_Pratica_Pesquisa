

# Perception of Personality Traits in Crowds of Virtual Humans

Lucas Nardino  
*Virtual Humans Lab*  
*School of Technology*  
*Pontifical Catholic University*  
*of Rio Grande do Sul*  
 Porto Alegre, Brazil  
 lucas.nardino@edu.pucrs.br

Diogo Schaffer  
*Virtual Humans Lab*  
*School of Technology*  
*Pontifical Catholic University*  
*of Rio Grande do Sul*  
 Porto Alegre, Brazil  
 diogo.schaffer@acad.pucrs.br

Felipe Elsner  
*Virtual Humans Lab*  
*School of Technology*  
*Pontifical Catholic University*  
*of Rio Grande do Sul*  
 Porto Alegre, Brazil  
 f.elsner@edu.pucrs.br

Enzo Krzmienski  
*Virtual Humans Lab*  
*School of Technology*  
*Pontifical Catholic University*  
*of Rio Grande do Sul*  
 Porto Alegre, Brazil  
 enzo.neves@edu.pucrs.br

Victor Flávio de Andrade Araujo  
*Virtual Humans Lab*  
*School of Technology*  
*Pontifical Catholic University*  
*of Rio Grande do Sul*  
 Porto Alegre, Brazil  
 victor.flavio@acad.pucrs.br

Gabriel Fonseca Silva  
*Virtual Humans Lab*  
*School of Technology*  
*Pontifical Catholic University*  
*of Rio Grande do Sul*  
 Porto Alegre, Brazil  
 gabriel.fonseca94@edu.pucrs.br

Vinícius Jurinic Cassol  
*Virtual Humans Lab*  
*School of Technology*  
*Pontifical Catholic University*  
*of Rio Grande do Sul*  
 Porto Alegre, Brazil  
 vinicius.cassol@edu.pucrs.br

Rodolfo Migon Favaretto  
*Virtual Humans Lab*  
*School of Technology*  
*Pontifical Catholic University*  
*of Rio Grande do Sul*  
 Porto Alegre, Brazil  
 rodolfo.favaretto@gmail.com

Soraia Raupp Musse  
*Virtual Humans Lab*  
*School of Technology*  
*Pontifical Catholic University*  
*of Rio Grande do Sul*  
 Porto Alegre, Brazil  
 soraia.musse@pucrs.br

**Abstract**—This paper proposes a perceptual visual analysis regarding the personality of virtual humans. Many studies have presented findings regarding the way human beings perceive virtual humans with respect to their faces, body animation, motion in the virtual environment and etc. We are interested in investigating the way people perceive visual manifestations of virtual humans' personality traits when they are interactive and organized in groups. Many applications in games and movies can benefit from the findings regarding the perceptual analysis with the main goal to provide more realistic characters and improve the users' experience. We provide experiments with subjects and obtained results indicate that, although is very subtle, people perceive more the extraversion (the personality trait that we measured), into the crowds of virtual humans, when interacting with virtual humans behaviors, than when just observing as a spectator camera.

**Index Terms**—crowd simulation, virtual agents, perception, personality traits.

## I. INTRODUCTION

Since the pioneer work proposed by Thalmann and Musse [1], many other methods were proposed for crowd simulation, each one with a significant contribution. There are methods that deal with crowds from a microscopic point of view [2], [3], as well methods that deal with a macroscopic

point of view [4], [5], and, even, methods that combine both microscopic and macroscopic simulation strategies [6]. Others explored how to compare crowds [7], high dense crowds [2], [8], heterogeneous behaviors [9], navigation control [10], and personality traits for agents [11]–[13].

Despite the great number of methods proposed for the most varied range of subjects concerning crowd simulation, only very few of them tackled the problem of perceptual analysis of behaviors in crowds. Indeed, human perception is essential for Computer Graphics (CG). Several techniques developed in the past were based on knowledge of human vision, for example, the interpretation of visual stimuli [14]. These stimuli generate information, which is processed and placed in a specific context. Human perception is a theme present in several researches in CG [14], [15], and it is considered very relevant when discussing the evolution of virtual humans. Virtual humans can be observed through stimuli such as images, videos, games, and Virtual Reality interactions, among others. For these virtual humans do not generate uncomfortable perceptions and falling into an Uncanny Valley [16], they need to present characteristics common to human beings, such as emotions, personality traits, interactions, expressions, etc.

Crowd perception is very important for learning about group

behavior, in which observers can see interpersonal interaction on a collective level [17]. The area of crowd perception has grown in recent years in several scientific researches (both through psychology and computing), such as perception of different models of agents in crowd simulations [18], perception of geometric and cultural features in virtual crowds [19], [20], perception of density in virtual crowds from two points of view [21], effects on users during interaction with a virtual crowd in an immersive virtual reality environment [22], studies of social categorization and emotions in crowds using ensemble coding [17], [23], among other researches. However, these methods do not focus on the perception of interactions between agents, between agent and user, and the impact of geometric personalities and emotions (that is, no facial and body expressions) on the perception of these interactions. In this work, we define three hypotheses we want to answer:

- $H_01$  defining that people with only observational control of agents in the crowd (do not interfere with crowd dynamics) perceive interactions similarly to people with control of agents in the crowd (the user is considered a crowd agent);
- $H_02$  defining that people with only observational control of crowd agents perceive different personalities and emotions similarly to people with control of crowd agents. In this case, as in our work we only use extraversion personality trait, different personalities mean that an agent can or cannot be extraverted;
- $H_03$  defining that the perception of interactions in crowds is not related to the perception of different personalities and emotions;

To try to answer the hypotheses, we created three scenarios with virtual crowds: *i*) Scenario 1, in which a user controlled a first-person camera throughout the entire scenario, without interfering with the agents' behavior; *ii*) Scenario 2, in which a user also controlled a first-person camera throughout the entire scenario, but he/she is considered as one agent of the simulated crowd, using the BioCrowds [24] model; *iii*) Scenario 3, in which a user is also an agent in the crowd, but the simulated crowd is different from Scenario 2 because we use an extension of BioCrowds model, called Normal Life behaviors [25], i.e., people are not in emergent situation. As the contribution of this paper, we introduced in BioCrowds the Extraversion factor to be distributed among the agents, so they are impacted by their levels of extraversion when applying their motion. Such factor is inspired in the personality traits methods, as proposed by Durupinar et al. [11]. From the observations and interactions with the scenarios, people answered questions about how they perceive the agents' interactions, and their different personalities and emotions, as discussed in this paper.

This paper is organized as follows: Section II presents the related work, while Section III presents the methodology proposed. Section IV presents the results achieved with our method and evaluation with subjects. Finally, Section VI presents the final considerations and future work of our method.

## II. RELATED WORK

This section discusses some work related to pedestrian and crowd behavioral analysis, focusing on personality traits, emotion, and perception. Knob et al. [26] presented work related to visualizing interactions between pedestrians in video sequences and virtual agents in crowd simulations. OCEAN-based factors gave interactions for each pedestrian and agent. OCEAN [27], [28] is the most commonly used personality trait model for this type of analysis, also known as the Big-Five: *O - Openness to experience*: "the active seeking and appreciation of new experiences"; *C - Conscientiousness*: "degree of organization, persistence, control, and motivation in goal directed-behavior"; *E - Extraversion*: "quantity and intensity of energy directed outwards in the social world"; *A - Agreeableness*: "the kinds of interaction an individual prefers from compassion to tough-mindedness"; *N - Neuroticism*: "how much prone to psychological distress the individual is" [29]. Durupinar et al. [30] also used OCEAN to visually represent personality traits.

Visual representation of agents is given in various ways, for example, the animations of agents are based on these two cultural features (OCEAN and emotion). If an agent is sad, the animation will represent that emotion. Yang et al. [21] conducted a study analyzing perception to determine the impact of groups at various densities, using two points of view: top and first-person. In addition to that, they examined what kind of camera position might be best for density perception.

Regarding realism perception, the work proposed by Araujo et al. [31] investigated people's perception of characters created using CG, comparing if they feel more comfortable with more recent CG characters or older ones. The authors found out that the perceived comfort about newer CG characters was more significant than the perception of comfort about older CG characters. Also, people's perception of comfort in 2020 was greater than people's perception in 2012.

In another work [19], [20], the authors evaluate the human perception regarding geometric features, personalities, and emotions in avatars. Results indicate that, even without explaining to the participants the concepts of cultural features and how they were calculated (considering the geometric features), in most cases, the participants perceived the personality and emotion expressed by avatars, even without faces and body expressions.

The work proposed by Volonte et al. [22] examined the effects on users during interaction with a virtual human crowd in an immersive virtual reality environment. They found that the users' were able to interpret the verbal and non-verbal behaviors of the virtual human characters where Positive emotional crowds elicit the highest scores in the variables related to interaction with the virtual characters.

Next, we present the proposed model to generate virtual agents with personality traits (in this case, we just used the extraversion personality trait) and how we evaluate the people's perception.

## III. PROPOSED MODEL

This section describes our model to provide agents endowed with personalities, in order to allow the simulation of realistic individuals. Firstly, in Section III-A we briefly describe BioCrowds [24], in Section III-B we describe the Normal Life [25] BioCrowds extension and finally, in Section III-C we detail the personality model.

### A. BioCrowds Model

BioCrowds [24] is a model for crowd simulations based on a space colonization algorithm designed to generate leaf venation patterns. In this model, a discrete space is populated by a set of marker points. Virtual agents compete for these markers based on a proximity criterion and capture range, effectively competing for the space in which they occupy and move. Indeed, each agent  $i$  accesses the markers inside its personal space  $R_i$  to search for markers that are closest to  $i$  than any other agent  $j$ . So, a marker is only available to the closest agent.

For a given agent  $i$ , with a set of  $N$  available markers  $S = \{a_1, a_2, \dots, a_N\}$ , we calculate its movement vector  $\vec{m}$  using Equation 1:

$$\vec{m} = \sum_{k=1}^N w_k (\vec{a}_k - \vec{x}), \quad (1)$$

where  $\vec{a}_k$  is the marker's position and  $\vec{x}$  is the agent's position.  $w_k$  is that marker's weight, calculated from Equation 2:

$$w_k = \frac{f(\vec{g} - \vec{x}, \vec{a}_k - \vec{x})}{\sum_{l=1}^N f(\vec{g} - \vec{x}, \vec{a}_l - \vec{x})}, \quad (2)$$

where  $\vec{g}$  is the position of agent  $i$  goal.

To determine function  $f$ , let us first assume that all markers  $\vec{a}_k$  affecting agent  $i$  are at the same distance  $\vec{a}_k - \vec{x}$  from this agent. Such function should prioritize markers that lead the agent directly to its goal, i.e., it should (i) reach its maximum when the (nondirected) angle  $\theta$  between  $\vec{g} - \vec{x}$  and  $\vec{a}_k - \vec{x}$  is equal to  $0^\circ$ ; (ii) reach its minimum when  $\theta = 180^\circ$ ; and (iii) decrease monotonically as  $\theta$  increases from  $0$  to  $180^\circ$ . Also, if the distances  $\vec{a}_k - \vec{x}$  differ, the markers further from the agent should have relatively smaller weights, to prevent them from dominating the computation of the tentative motion vector  $\vec{m}$ . A possible choice for  $f$  that satisfies these assumptions is defined in Equation 3:

$$f(x, y) = \frac{1 + \cos\theta}{1 + \|y\|}, \quad (3)$$

where  $\theta$  is the angle between  $x$  and  $y$ . Please refer to BioCrowds original paper [24] for further details about the method.

The weights will cause the agent to move towards its goal as long as there are markers available along the way. An agent's movement will be blocked by the absence of markers.

### B. BioCrowds Normal Life

Helbing et al. [32] present some of the main characteristics of people in normal life evacuations:

- In general, pedestrians take into account detours as well as the comfort of walking, thereby minimizing the effort to reach their destination;
- Pedestrians prefer to walk with an individual desired speed, which corresponds to the most comfortable walking speed as long as it is not necessary to go faster in order to reach the destination in time;
- Pedestrians keep a certain distance from other pedestrians and borders.

Using BioCrowds, Rockenbach et al. [25] proposed an extension to provide crowds that achieve the main characteristics of normal life [32]. In this case, the Normal Life behavior aims to improve the realism of agents' behaviors in evacuation scenarios. If we imagine that agents want to evacuate the environment, but without stress, i.e., it is not a panic situation, people will apply some behaviors that are different from the ones applied during a hazardous scenario. In this model, the authors proposed the term comfort ( $c$ ) as a function of the available area for each agent. According to Helbing et al. [32], this area is smaller the more a pedestrian is in a hurry, and still decreases with higher pedestrian density. As proposed in previous work [25], in our method, the sense of personal area was adapted to the number of markers  $N_i$  each agent  $i$  has. So,  $c_i$  is defined as a function of the number of available markers (the set  $S_i$ ) a certain agent  $i$  has. If the number of markers  $N_i$  decreases, then  $c_i$  decreases too. So, the agent will gradually shift its focus from its designated goal to looking for a more comfortable space i.e., with more available markers. Actually, we normalize  $N_i$  dividing by the maximum number of markers  $M$  (empirically defined as 70, once it is impacted by the world configurations).

With this definition, the comfort factor is in the interval  $[0; 1]$  for agent  $i$ , according to Equation 4:

$$c_i = \frac{N_i}{M}. \quad (4)$$

The original BioCrowds [24] model computes the weight of each marker, as defined in Equation 2, by comparing the angle difference between the direction defined from the agent towards its goals and all available markers. In Normal Life BioCrowds [25], the markers weights are computed in order to endow agents with the previously described behavior, i.e to look for a more comfortable space. The new weight affected by comfort ( $w'_k$ ) for agent  $i$  is defined by Equation 5:

$$w'_{k,i} = \delta_i \cdot w_{k,i} + (1 - \delta_i), \quad (5)$$

where  $w_{k,i}$  is the original weight calculated by BioCrowds in Equation 2 and  $\delta_i$  is the comfort bias for agent  $i$  defined by Equation 6:

$$\delta_i = \sin(c_i \cdot \frac{\pi}{2}). \quad (6)$$

Related to Equation 5, agents behave according to original BioCrowds when  $\delta_i = 1$ , i.e., markers weights vary according to the goal direction. However, when the number of markers decreases, the bias decreases as well, resulting in their weights being more similar, causing the agent to go towards the available markers, even if those do not lead to the goal.

While crowd behaviors are studied in various scenarios, it is acceptable that various "normal life scenarios" can be different in real life. One possibility is that the crowd is affected by the personality traits of membership and not only responsive to the space around the subjects. This is the main goal of our work and the methodology to achieve that is presented in the next section.

### C. Extraversion Personality Trait

In order to include personality traits in our agents, we chose the OCEAN (Openness to experience, Conscientiousness, Extraversion, Agreeableness, Neuroticism) psychological traits model, proposed by Goldberg [33], once it is the most accepted model to define the personality of a person.

In this work, we focused on modeling the Extraversion trait, which reflects the sociability and talkativeness but also, in the geometric sense, how comfortable the individual is around crowds and other groups [34]. So, the Extraversion trait can affect the comfort of an agent when interacting with a user's avatar. The Normal Life model dictates how much the agents value their personal space in comparison to the desire to reach their goals. We propose an Extraversion factor  $E_i$ , for agent  $i$ , which influences the generated behaviors according to each agent's personality, as to vary how comfortable the agent is with a crowded personal space. We can see in Equation 7 the modified Normal Life Equation 5, including the Extraversion factor included.

$$w'_{k,i} = \delta_i \cdot w_{k,i} \cdot E_i + (1 - \delta_i) \cdot (1 - E_i). \quad (7)$$

Fig. 1 illustrates three situations using BioCrowds with 50 agents positioned around a goal. On the left, we have our extended model of BioCrowds with two different levels of Extraversion. We use 25 agents having 0.8 as Extraversion values and 25 agents with 1.0. In the center, we have the implementation of Normal Life model, according to Rockenbach et al. [25]. It is easy to remark that agents are well distributed in the space trying to maximize their comfort. Finally, on the right, we have the original BioCrowds model. It is easy to perceive how the personality changes the model results. On the left of Fig. 1, one can perceive the distribution of agents, where the ones with higher values of Extraversion are close to each other, and also close to the goal, because they were not disturbed by the presence of other agents, so they went directly to the goal. Still, in the image on the left, we can see the agents with lower Extraversion far from the goal and far from each other, as well. It is important to notice that lower values of  $E$  are possible. However, agents, in those cases, can behave going far from the goal, and then subjects can easily perceive the difference. That is why we use values where agents still go towards the goal.

![](451417078c55730b2f3f9ea924407f78_img.jpg)

Fig. 1. Three applications of BioCrowds. The yellow dots represent the agents' goals. Left: BioCrowds with our proposed model of the Extraversion personality trait, using two distinct agent profiles: agents with  $E = 1$  (closer to the goal and to each other); and agents with  $E = 0.8$  (further from the goal and each other). Center: BioCrowds with Normal Life, as proposed by Rockenbach et al. [25]. Right: the original BioCrowds model, as proposed by Bicho et al. [24].

Fig. 2 through 5 present the evolution of four simulations, containing 50 agents each, using different methods. Fig. 2 presents a simulation of our proposed model of Extraversion, using two distinct agent profiles: agents with  $E = 1$  (highlighted in blue); and agents with  $E = 0.8$  (highlighted in green). Fig. 3 presents a simulation of our proposed model of Extraversion with all agents having  $E = 0.8$ . Fig. 4 presents a simulation utilizing the Normal Life extension model, as proposed by Rockenbach et al. [25]. Finally, Fig. 5 presents a simulation utilizing the original BioCrowds model, as proposed by Bicho et al. [24].

In Fig. 2(c), we can see that the agents with higher  $E$  occupy less space, and tend to cluster together, whilst agents with lower  $E$  occupy more space and keep a certain distance from each other. In Fig. 3(c), we can observe agents with a lower value of Extraversion, where they tend to further themselves when disturbed by the presence of others, while still aiming for the goal. Similar behavior is perceived in Fig. 4(c), with agents being more distributed in order to maximize their comfort. Finally, Fig. 5(c) presents agents that do not take comfort and Extraversion into account, allowing them to be closer to one another and cluster around the goal.

## IV. RESULTS

This section presents the obtained results when evaluating the people perception.

### A. Research Methods

We developed a survey in Google Forms to understand how people perceive crowds personalities in our experiments. The survey was answered by 31 people, where 22.6% are women and 74.4% are men. Other demographic attributes are following specified. Regarding the educational level, 58.06% of the population have completed high school, and 41.96% have higher education. With respect to subjects' age, the average is 21.645, therefore, people below and above average are respectively 80.6% and 19.4%. Subject with familiarity with CG is 22.6% of the population, while 77.4% declare themselves as non-familiar with CG. Initially, we informed people that they were free to give up responding in case of tiredness, boredom, or dizziness. In

![](bb08c83fc8939517c6803d65c69dd06b_img.jpg)

Fig. 2. Evolution of a simulation utilizing our proposed model of the Extraversion (E) personality trait. Two agent profiles are presented: 25 agents with  $E = 1$  (blue); and 25 agents with  $E = 0.8$  (green). The frames 150 (a), 450 (b) and 1500 (c) are presented.

![](edd10d3006553f0a7a5a7f844ed8cd01_img.jpg)

Fig. 3. Evolution of a simulation utilizing our proposed model of the Extraversion (E) personality trait. All 50 agent present the value of  $E = 0.8$ . The frames 150 (a), 450 (b) and 1500 (c) are presented.

![](8e065eabc40a3645a569db7e9cd0da32_img.jpg)

Fig. 4. Evolution of a simulation utilizing the Normal Life extension model, as proposed by Rockenbach et al. [25]. The frames 150 (a), 450 (b) and 1500 (c) are presented.

![](0fd25e7fd1bbb3a7e2e304fc28a8496b_img.jpg)

Fig. 5. Evolution of a simulation utilizing the original BioCrowds model, as proposed by Bicho et al. [24]. The frames 150 (a), 450 (b) and 1500 (c) are presented.

addition, we asked people if they agreed to participate into the survey. The experiments were organized in three scenarios:

1) *Scenario 1 - The user only observes the crowd:* In this scenario, the user observes the environment and the movement of the agents, using a spectator camera that does not affect the agents' behaviors.

2) *Scenario 2 - The avatar is one agent in the original BioCrowds:* In this scenario, the user can interact with the agents whilst being able to occupy space while walking. In this Scenario, the agents take the user's presence into account and treat her/him as a BioCrowds agent.

3) *Scenario 3 - The avatar is one agent in BioCrowds Normal Life:* In this scenario, the user also can interact with the agents, as in Scenario 2, however, the agent wants to be comfortable in the space, as applied in the Normal Life model. In this scenario, the agents consider the user's presence treating her/him as a Normal Life agent.

The main difference between the avatar in Scenarios 2 and 3 is that in Scenario 2, agents or the avatar always replicate the main rule of BioCrowds, i.e., markers on the floor are attributed to the closest agent (weighted motion vectors use Equations 1 and 2. In Scenario 3, the markers are attributed to the agents (and the avatar) depending on their Extraversion values, as described in Equation 7.

The scenarios were developed in the Unity3D engine and presented in a WebGL application. So, people accessed the scenarios through a GitHub link distributed by Google Forms. We informed people that they could move freely between the survey link and the application link. After each scenario, the user had to answer two questions that reflect her/his perception throughout the simulation:

- A) "Did you notice interactions between the agents?"
- B) "Did you perceive different emotions or personalities in the agents?"

Both questions were answered using 5-Likert Scales ("Did not notice at all" to "Noticed completely"), as shown in Fig. 6 and 7. Question A was asked to evaluate  $H0_1$ , question B to evaluate  $H0_2$ . With respect to  $H0_3$ , we measured the relationships between the results of questions A and B. The next section presents our findings with respect to the applied experiments and surveys.

### B. Research Results

Based on our results, we found that the perception of the extraversion personality trait of virtual agents and the interaction between them depends on the user's form of interaction, as further discussed in this section. In Scenario 1, where the user could only observe the agents, few users perceived interactions between agents or agent's emotions and personalities. On the other hand, in Scenarios 2 and 3, in which users could interact with agents with a virtual physical body, the users perceived agents interacting among themselves, as

![](29ac39bfd74e57a92045649f83cad949_img.jpg)

Fig. 6. Answers collected from the form question ("Did you notice interactions between the agents?"), regarding Scenarios 1, 2 and 3.

![](096d7a8a21933900dad68d82ae8a97fb_img.jpg)

Fig. 7. Answers collected from the form question ("Did you perceive different emotions or personalities between the agents?"), regarding Scenarios 1, 2 and 3.

shown in Fig. 6, but had difficulties identifying emotions and personalities, as shown in Fig. 7.

In addition, in order to answer the hypotheses presented in Section I, we performed statistical analysis using the *Mann-Whitney* test of hypotheses (to evaluate  $H0_1$  and  $H0_2$ ) and *Spearman* correlations (to evaluate  $H0_3$ ) through the Scipy library in the Python language, using 95% significance level. Both the Mann-Whitney test and the Spearman correlation were used because they are robust methods for unbalanced samples, such as the percentage of male participants was higher than the percentage of female participants obtained in our results. For these analyses, we scored the Likert Scales from 1 (Did not notice at all) to 5 (Noticed completely), and for the hypothesis tests, we used the averages of these scores (averages presented in Table I). We performed a general analysis and using demographic profiles. In the hypotheses tests, we compared the results as follows: *i*) Relating to the applied scenarios, for example, the average of the Likert scores answers from question A in Scenario 1 vs. the average of the answers from question A in Scenario 2. So this was made using the three Scenarios (1, 2, 3) x the two questions (A and B) x demographics (gender, familiarity with CG, education level, age), resulting in 24 analyzed configurations; *ii*) Relating to the demographic data, for example, the average of women's answers to question A x the average of men's answers to question A. In this case, we compared the overall averages,

that is, answers to questions A and B taking into account all Scenarios (1, 2, 3), and the averages of the questions taking into account the scenarios separately. With respect to correlations, we measured the relationships between questions A's answers and B's answers. Regarding the general analysis, in the first four lines of Table I, (without separating into demographic data), we only found significant results when comparing the averages of question A ( $H0_1$ ) between Scenarios 1 and 2 ( $p$ -value .01). Therefore, **we can say that people perceived more interactions between agents in Scenario 2 than in Scenario 1.** Regarding Spearman's correlations, we found two significant  $p$ -values in the general (.018 in all Scenarios) and Scenario 3 (.03) analysis between questions A and B ( $H0_3$ ). However, the correlation values were low, being .245 in the general and .39 in Scenario 3. As in the general analysis, the correlation value was below .3, so **we can say that there was a weak correlation in Scenario 3 between the answers of A and B. Therefore, in Scenario 3, we can say that there was a weak tendency that the more people perceived interactions, the more they perceived that agents had different personalities and emotions (and vice versa).**

With respect to gender (we excluded a person from this analysis, as that person did not declare their gender), we did not find significant results when we evaluated the women's data (both in the hypothesis tests and in the correlations). In this case, we may not have found significant results because the number of female participants was very low compared to the number of male participants. As in the general analysis, we only found a significant result when we compared the averages of question A ( $H0_1$ ) between Scenarios 1 and 2 (.018). So, **we can say that men perceived more interactions in Scenario 2 than in Scenario 1.** The results of the correlations were also similar to the results of the general analysis, that is, significant  $p$ -values in the correlations between questions A and B ( $H0_3$ ) taking into account all scenarios together (.036), and taking into account only Scenario 3 (.03). The correlation values were, respectively, .253 and .454. Therefore, **looking only at the correlation between A and B's answers in Scenario 3, we can say that there was a weak trend that the more men perceived the interactions, the more they perceived that agents had different personalities and emotions (and vice versa).** We did not find significant results in comparisons between data from women vs. data from men.

With respect to educational level, for people with complete high school, we found significant  $p$ -values (.002 and .046) in the weak correlations (.409 and .475) between questions A and B ( $H0_3$ ) for the scenarios in general and Scenario 3 separately. With that, **we can say that in general and in Scenario 3, there was a weak tendency that the more people with complete high school perceived the interactions, the more they perceived that the agents had different personalities and emotions (and vice versa).** For people with higher education, we only found a significant result (.01) in the comparison between Scenarios 1 and 2 in question A ( $H0_1$ ). Therefore, **we can say that people with higher education perceived more interactions in Scenario 2 than in 1.**

TABLE I  
TABLE OF AVERAGE OF QUESTIONS A AND B (USING LIKERT SCALES AS SCORES) IN ALL ANALYSIS (GENERAL, GENDER, EDUCATION, AGE, FAMILIARITY WITH CG).

| Analysis                     | Scenario    | Question A (AVG) | Question B (AVG) |
|------------------------------|-------------|------------------|------------------|
| General                      | All (1,2,3) | 2.473            | 2.043            |
| General                      | 1           | 2.129            | 2.032            |
| General                      | 2           | 2.71             | 1.903            |
| General                      | 3           | 2.581            | 2.194            |
| Analysis (Gender)            | Scenario    | Question A (AVG) | Question B (AVG) |
| Women                        | All (1,2,3) | 2.476            | 2.449            |
| Women                        | 1           | 2.143            | 2.286            |
| Women                        | 2           | 2.714            | 2.143            |
| Women                        | 3           | 2.571            | 2.286            |
| Men                          | All (1,2,3) | 2.449            | 1.942            |
| Men                          | 1           | 2.087            | 1.913            |
| Men                          | 2           | 2.696            | 1.783            |
| Men                          | 3           | 2.565            | 2.13             |
| Analysis (Education)         | Scenario    | Question A (AVG) | Question B (AVG) |
| Complete High School         | All (1,2,3) | 2.389            | 2.185            |
| Complete High School         | 1           | 2.167            | 2.111            |
| Complete High School         | 2           | 2.5              | 2.111            |
| Complete High School         | 3           | 2.5              | 2.333            |
| Higher Education             | All (1,2,3) | 2.59             | 1.846            |
| Higher Education             | 1           | 2.077            | 1.923            |
| Higher Education             | 2           | 3.0              | 1.165            |
| Higher Education             | 3           | 2.692            | 2.0              |
| Analysis (Age)               | Scenario    | Question A (AVG) | Question B (AVG) |
| < 21.645                     | All (1,2,3) | 2.373            | 2.08             |
| < 21.645                     | 1           | 2.12             | 2.04             |
| < 21.645                     | 2           | 2.6              | 2.0              |
| < 21.645                     | 3           | 2.4              | 2.2              |
| > 21.645                     | All (1,2,3) | 2.889            | 1.889            |
| > 21.645                     | 1           | 2.167            | 2.0              |
| > 21.645                     | 2           | 3.167            | 1.5              |
| > 21.645                     | 3           | 3.333            | 2.167            |
| Analysis (CG Familiarity)    | Scenario    | Question A (AVG) | Question B (AVG) |
| With Familiarity             | All (1,2,3) | 2.762            | 2.095            |
| With Familiarity             | 1           | 2.286            | 2.286            |
| With Familiarity             | 2           | 3.0              | 2.0              |
| With Familiarity             | 3           | 3.0              | 2.571            |
| Did not know or not familiar | All (1,2,3) | 2.389            | 1.972            |
| Did not know or not familiar | 1           | 2.083            | 1.958            |
| Did not know or not familiar | 2           | 2.625            | 1.875            |
| Did not know or not familiar | 3           | 2.458            | 2.083            |

Comparing the answers of people with complete high school Vs. people with higher education, we found significant results in the comparisons of question B ( $H0_2$ ) related to the general analysis (.037) and the analysis of Scenario 2 (.034). With this,

in general (and in Scenario 2), we can say that people with complete high school perceived more different personalities and emotions in agents than people with higher education.

Regarding age, for below-average people, we found a significant result (.037) in the comparison between Scenarios 1 and 2 in question A ( $H0_1$ ). We also found a significant  $p$ -value (.016) in the general correlation (all Scenarios) between questions A and B ( $H0_3$ ), but the correlation value (.278) was too low. Therefore, **we can only say that people below the average age perceived more interactions between agents in Scenario 2 than in Scenario 1.** Regarding above-average people, we only found significant results (.035) when comparing Scenarios 1 and 2 in question B ( $H0_2$ ). Therefore, **we can say that above the average age people perceived more different personalities and emotions in agents in Scenario 1 than in Scenario 2.** Comparing people below the average age vs. above, we only found significant results in question A ( $H0_1$ ) when we analyzed in general (.037) and in Scenario 3 (.036). With that, **we can say that in general (and in Scenario 3), above the average age people perceived more interactions between agents than below.**

Regarding familiarity with CG, we only found a significant  $p$ -value in the correlation between the answers to questions A and B ( $H0_3$ ) in Scenario 3 (.026), and differently from the previous results, having a strong correlation of .814. Thus, **in Scenario 3, we can say that there was a strong tendency that the more people familiar with CG perceived interactions, the more they perceived that agents had different personalities or emotions (vice versa).**

Regarding people who did not know or were not familiar with CG, we only found a significant result in the comparison between Scenarios 1 and 2 in question A (.02), i.e.,  $H0_1$ . With that, **we can say that these people perceived more interactions in Scenario 2 than in Scenario 1.** In the comparison between the two groups (people with familiarity with CG vs. people who did not know or were not familiar with CG), we did not find significant results.

## V. DISCUSSION

In this section, we report our discussions of the results presented in the previous section with respect to the three research hypotheses. Remembering, the hypotheses are: *i*)  $H0_1$  defining that people with only observational control of agents in the crowd (do not interfere with crowd dynamics) perceive interactions similarly to people with control of agents in the crowd (the user is considered a crowd agent); *ii*)  $H0_2$  defining that people with only observational control of crowd agents perceive different personalities and emotions similarly to people with control of crowd agents; *iii*)  $H0_3$  defining that the perception of interactions in crowds is not related to the perception of different personalities and emotions.

Regarding  $H0_1$  (perception of interactions), people in general (also separately - men, higher education, people below the average age, and who did not know or were not familiar with CG), perceived more interactions in Scenario 2 than in Scenario 1. In the other comparisons between scenarios (1 vs.

3 and 2 vs. 3), we did not find significant results. However, if we look only at the averages of the general analysis in Table I, we can see that Scenario 2 had the highest average values of perception in question A. These results refute  $H0_1$  and tell us that people perceived more interactions when they were part of the interactions (looking for their spaces) than when they just watched the agents interacting. In addition, we found an age effect, where people above average age perceived more interactions than people below average age.

Regarding  $H0_2$  (perception of different personalities and emotions), we only found significant results when we separated people by age. People who were above the average age perceived more different personalities and emotions in Scenario 1 than in Scenario 2. This result is interesting because users only observed the agents in scenario 1, that is, the camera did not influence the interactions. However, if we look only at the averages (Table I), we can see that all question B average values of perception of different personalities and emotions for Scenario 2 were the lowest compared to the other scenarios. Scenario 3 was the one with the highest perception values of different personalities and emotions. These results may indicate that the perception of different personalities and emotions can be difficult when the user personified an agent interfering with the movement of other agents. However, the values of perceptions increase when agents consider people to be Normal Life agents. Thus, we can say that these results refute  $H0_2$ . In addition, we found an educational level effect, in which people with complete high school perceived more different personalities and emotions than people with higher education.

Regarding  $H0_3$ , we found relationships between perceiving interactions and perceiving different personalities and emotions. In most cases, these results had a low correlation and occurred in Scenario 3 (for men and people with complete high school). We also found a strong correlation when we analyzed data from people familiar with CG. This is an interesting result and refutes the hypothesis, as it means that the person who is familiar with CG tends to find a relationship between interactions (between agents) with different personalities and emotions. This makes sense, as people familiar with CG may be used to observing interactions between agents, personality traits, and emotions in simulations, in games, etc. Furthermore, taking into account that this result happened in Scenario 3, which was related to Normal Life, people familiar with CG may also be used to interact in virtual environments in which agents take into account the participant's presence, such as in games. In relation to games, the behavior of characters that is closer to reality can improve the game experience. Taking into account our result, we should think that people can perceive extraversion in motion animations, for example, the perception of an extraverted character heading towards a group of friends.

## VI. FINAL CONSIDERATIONS

In our research, the data collected indicates that one of the key factors in the perception of users is the kind of interaction they have with the virtual environment. Moreover,

we found out that users tended to only perceive interactions and extraversion personality trait on the scenarios when they actively interacted with the agents.

This paper has some limitations: firstly, we use only one simulation scenario, i.e., agents enter the environment and go to the goal, in all tested situations. Other experiments could enrich the tests and maybe the conclusions. Also, the number of agents could vary as to their appearance and animation. In addition, we could try to have more users to better sustain our hypotheses. For future work, we plan to model all of the OCEAN factors to influence the agents' geometric behavior. As discussed in this paper, the geometric factors are only perceived when the user is actively interacting with the agents, to remedy this we plan to add facial expressions for the agents to complement the geometric factors. Also, we plan to implement more than one physical appearance for the agents, so the realism can be increased. In addition, including more variety to the visual representation of the agent, that is, adding more 3D models of people, as to increase simulation diversity and realism, are part of our plans for the future.

## ACKNOWLEDGMENT

The authors would like to thank CNPq and CAPES for partially funding this work.

## REFERENCES

- [1] S. R. Musse and D. Thalmann, "A model of human crowd behavior : Group inter-relationship and collision detection analysis," in *Computer Animation and Simulation '97*, D. Thalmann and M. van de Panne, Eds. Vienna: Springer Vienna, 1997, pp. 39–51.
- [2] N. Pelechano, J. M. Allbeck, and N. I. Badler, "Controlling individual agents in high-density crowd simulation," in *Proceedings of the 2007 ACM SIGGRAPH/Eurographics Symposium on Computer Animation*, ser. SCA '07. Aire-la-Ville, Switzerland, Switzerland: Eurographics Association, 2007, pp. 99–108.
- [3] C. W. Reynolds, "Flocks, herds and schools: A distributed behavioral model," *SIGGRAPH Comput. Graph.*, vol. 21, no. 4, pp. 25–34, Aug. 1987.
- [4] R. L. Hughes, "A continuum theory for the flow of pedestrians," *Transportation Research Part B: Methodological*, vol. 36, no. 6, pp. 507–535, 2002.
- [5] A. Treuille, S. Cooper, and Z. Popović, "Continuum crowds," *ACM Trans. Graph.*, vol. 25, no. 3, pp. 1160–1168, Jul. 2006.
- [6] A. D. S. Antonitsch, D. H. M. Schaffer, G. W. Rockenbach, P. Knob, and S. R. Musse, "Bioclouds: A multi-level model to simulate and visualize large crowds," in *Computer Graphics International Conference*. Springer, 2019, pp. 15–27.
- [7] S. R. Musse, V. J. Cassol, and C. R. Jung, "Towards a quantitative approach for comparing crowds," *Computer Animation and Virtual Worlds*, vol. 23, no. 1, pp. 49–57, 2012.
- [8] R. Narain, A. Golas, S. Curtis, and M. C. Lin, "Aggregate dynamics for dense crowd simulation," *ACM Trans. Graph.*, vol. 28, no. 5, pp. 122:1–122:8, Dec. 2009.
- [9] L. Zheng, D. Qin, Y. Cheng, L. Wang, and L. Li, "Simulating heterogeneous crowds from a physiological perspective," *Neurocomput.*, vol. 172, no. C, pp. 180–188, Jan. 2016.
- [10] S. Paris, J. Petr  , and S. Donikian, "Pedestrian reactive navigation for crowd simulation: a predictive approach," *Comput. Graph. Forum*, vol. 26, pp. 665–674, 2007.
- [11] F. Durupinar, N. Pelechano, J. Allbeck, U. G  d  kbay, and N. I. Badler, "How the ocean personality model affects the perception of crowds," *IEEE Computer Graphics and Applications*, vol. 31, no. 3, pp. 22–31, 2009.
- [12] P. Knob, M. Balotin, and S. R. Musse, "Simulating crowds with ocean personality traits," in *Proceedings of the 18th international conference on intelligent virtual agents*, 2018, pp. 233–238.
- [13] G. F. Silva, P. Knob, D. A. Schlatter, C. G. Johansson, and S. R. Musse, "Moving virtual agents forward in space and time," in *19th Brazilian Symposium on Computer Games and Digital Entertainment, SBGames 2020, Recife, Brazil, November 7-10, 2020*. IEEE, 2020, pp. 1–10.
- [14] E. Zell, K. Zibrek, and R. McDonnell, "Perception of virtual characters," in *ACM SIGGRAPH 2019 Courses*, 2019, pp. 1–17.
- [15] E. Zell, C. Aliaga, A. Jarabo, K. Zibrek, D. Gutierrez, R. McDonnell, and M. Botsch, "To stylize or not to stylize?: the effect of shape and material stylization on the perception of computer-generated faces," *ACM Transactions on Graphics (TOG)*, vol. 34, no. 6, p. 184, 2015.
- [16] M. Mori, "Bukimi no tani [the uncanny valley]," *Energy*, vol. 7, pp. 33–35, 1970.
- [17] S. A. Lamer, T. D. Sweeny, M. L. Dyer, and M. Weisbuch, "Rapid visual perception of interracial crowds: Racial category learning from emotional segregation," *Journal of Experimental Psychology: General*, vol. 147, no. 5, p. 683, 2018.
- [18] R. McDonnell, M. Larkin, S. Dobbyn, S. Collins, and C. O'Sullivan, "Clone attack! perception of crowd variety," in *ACM SIGGRAPH 2008 papers*, 2008, pp. 1–8.
- [19] V. Araujo, R. Migon Favaretto, P. Knob, S. Raupp Musse, F. Vilanova, and A. Brandelli Costa, "How much do you perceive this? an analysis on perceptions of geometric features, personalities and emotions in virtual humans," in *Proceedings of the 19th ACM International Conference on Intelligent Virtual Agents*, 2019, pp. 179–181.
- [20] V. Araujo, B. Dalmoro, R. Favaretto, F. Vilanova, A. Costa, and S. Musse, "How much do we perceive geometric features, personalities and emotions in avatars?" in *2021 Computer Graphics International (CGI2021)*, 2021, pp. xx–x.
- [21] F. Yang, J. Shabo, A. Qureshi, and C. Peters, "Do you see groups?: The impact of crowd density and viewpoint on the perception of groups," in *IVA*. ACM, 2018, pp. 313–318.
- [22] M. Volonte, Y.-C. Hsu, K.-Y. Liu, J. P. Mazer, S.-K. Wong, and S. V. Babu, "Effects of interacting with a crowd of emotional virtual humans on users' affective and non-verbal behaviors," in *2020 IEEE Conference on Virtual Reality and 3D User Interfaces (VR)*, 2020, pp. 293–302.
- [23] A. Goldenberg, E. Weisz, T. D. Sweeny, M. Cikara, and J. J. Gross, "The crowd-emotion-amplification effect," *Psychological science*, vol. 32, no. 3, pp. 437–450, 2021.
- [24] A. de Lima Bicho, R. A. Rodrigues, S. R. Musse, C. R. Jung, M. Paravisi, and L. P. Magalh  es, "Simulating crowds based on a space colonization algorithm," *Computers & Graphics*, vol. 36, no. 2, pp. 70–79, 2012.
- [25] G. Rockenbach, C. Boeira, D. Schaffer, A. Antonitsch, and S. R. Musse, "Simulating crowd evacuation: From comfort to panic situations," in *Proceedings of the 18th International Conference on Intelligent Virtual Agents*, ser. IVA '18. New York, NY, USA: Association for Computing Machinery, 2018, p. 295–300.
- [26] P. Knob, V. F. de Andrade Araujo, R. M. Favaretto, and S. R. Musse, "Visualization of interactions in crowd simulation and video sequences."
- [27] J. M. Digman, "Personality structure: Emergence of the five-factor model," *Annual Review of Psychology*, vol. 41, pp. 417–440, 1990.
- [28] O. P. John, *The "Big Five" factor taxonomy: Dimensions of personality in the natural language and in questionnaires*. New York, NY: 66–100, 1990, ch. 4, pp. 66–100.
- [29] W. Lord, *Neo Pi-R – A Guide to Interpretation and Feedback in a Work Context*, 1st ed. Hogrefe Ltd, 2007.
- [30] F. Durupinar, U. G  d  kbay, A. Aman, and N. I. Badler, "Psychological parameters for crowd simulation: From audiences to mobs," *IEEE TVCG*, vol. 22, no. 9, pp. 2145–2159, 2016.
- [31] V. Araujo, J. Melgare, B. Dalmoro, and S. R. Musse, "Is the perceived comfort with cg characters increasing with their novelty," *IEEE Computer Graphics and Applications*, 2021.
- [32] D. Helbing, I. J. Farkas, P. Molnar, and T. Vicsek, "Simulation of pedestrian crowds in normal and evacuation situations," in *International conference, Pedestrian and evacuation dynamics*, 2001, pp. 21–58.
- [33] L. R. Goldberg, "An alternative" description of personality": the big-five factor structure," *Journal of personality and social psychology*, vol. 59, no. 6, p. 1216, 1990.
- [34] R. M. Favaretto, L. Duhl, S. R. Musse, F. Vilanova, and A. B. Costa, "Using big five personality model to detect cultural aspects in crowds," in *2017 30th SIBGRAPI Conference on Graphics, Patterns and Images (SIBGRAPI)*. IEEE, 2017, pp. 223–229.