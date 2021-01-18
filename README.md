### SimpleJira

Библиотека для работы с Jira и автоматического тестирования кода, написанного на .NET, взаимодействующего с Jira.

### Features

- Удобный прокси для выполнения основных сценариев работы с жирой: создания нового issue, модификации существующего issue, поиск issue по JQL, добавления нового комментария к issue, получения списка комментариев к issue, загрузки вложений, скачивания вложений, получения списка transitions, вызова transition;
- Поддержка C# Mapping классов для удобной работы с полями issue;
- InMemory реализация прокси с поддержкой интерпретации JQL на объектах, хранящихся в памяти;
- Файловая реализация прокси с поддержкой интерпретации JQL на объектах, хранящихся на диске локальной машины;
- Linq Provider, работающий как с InMemory реализацией прокси так и с настоящей реалзицией прокси.

### NuGet

Для установки [SimpleJira пакета](https://www.nuget.org/packages/SimpleJira/), выполните следующую команду в [NuGet-консоли](http://docs.nuget.org/docs/start-here/using-the-package-manager-console)

    PM> Install-Package SimpleJira
    
или

    dotnet add package SimpleJira

для .NET CLI.

Для установки [SimpleJira.Fakes пакета](https://www.nuget.org/packages/SimpleJira.Fakes/), выполните следующую команду в [NuGet-консоли](http://docs.nuget.org/docs/start-here/using-the-package-manager-console)

    PM> Install-Package SimpleJira.Fakes
    
или

    dotnet add package SimpleJira.Fakes

для .NET CLI.

## Прокси

Взаимодействие с Jira происходит через интерфейс IJira. Для того чтобы инстанциировать его необходимо выполнить следущий код

```c#
IJira jira = Jira.RestApi("http://myjira.com", "my_user", "my_password");
```

#### Создание нового issue

```c#
JiraIssue issue = new JiraIssue {
    Project = new JiraProject {
        Key = "KNOPKLIENT"
    },
    IssueType = new JiraIssueType {
        Id = "22",
        Name = "Клиент"
    },
    Summary = "Новый клиент",
    Description = "Описание нового клиента",
    Assignee = new JiraUser {
        Key = "coolwage",
    },
    DueDate = DateTime.Today
};
issue.CustomFields[11323].Set("ИНН клиента");

JiraIssueReference reference = await jira.CreateIssueAsync(issue, cancellationToken);
```

#### Модификация issue

```c#
JiraIssueReference referene = JiraIssueReference.FromKey("KNOPKLIENT-234761");
JiraIssue issue = new JiraIssue {
    DueDate = DateTime.Today.AddDays(1)
};
await jira.UpdateIssueAsync(reference, issue, cancellationToken);
```

#### Поиск issue

```c#
string jql = "assignee = coolwage";
JiraIssuesRequest request = new JiraIssuesRequest {
    Jql = jql,
    StartAt = 0,
    MaxResults = 200,
    Fields = new string[] {"duedate", "summary", "assignee"}
};

JiraIssuesResponse response = await jira.SelectIssuesAsync(request, cancellationToken); 
```

#### Добавление комментария к issue

```c#
JiraIssueReference referene = JiraIssueReference.FromKey("KNOPKLIENT-234761");
JiraComment comment = new JiraComment {
    Body = "Новый комментарий"
};

JiraComment newComment = await jira.AddCommentAsync(reference, comment, cancellationToken);
```

#### Получение комментариев к issue

```c#
JiraIssueReference referene = JiraIssueReference.FromKey("KNOPKLIENT-234761");
JiraCommentsRequest request = new JiraCommentsRequest {
    StartAt = 0,
    MaxResults = 200
};

JiraCommentsResponse response = await jira.GetCommentsAsync(reference, request, cancellationToken);
```

#### Загрузка вложений

```c#
JiraIssueReference referene = JiraIssueReference.FromKey("KNOPKLIENT-234761");
byte[] bytes = GetContent();

JiraAttachment attachment = await jira.UploadAttachmentAsync(reference, "sample.txt", bytes, cancellationToken);
```

#### Скачивание вложений

```c#
JiraAttachment attachment = new JiraAttachment {
    Id = "2467578"
};

byte[] bytes = await jira.DownloadAttachmentAsync(attachment, cancellationToken);
```

#### Удаление вложений

```c#
JiraAttachment attachment = new JiraAttachment {
    Id = "2467578"
};

await jira.DeleteAttachmentAsync(attachment, cancellationToken);
```

#### Получение списка transitions

```c#
JiraIssueReference referene = JiraIssueReference.FromKey("KNOPKLIENT-234761");

JiraTransition[] transitions = await jira.GetTransitionsAsync(reference, cancellationToken);
```

#### Выполнение transition
```c#
JiraIssueReference referene = JiraIssueReference.FromKey("KNOPKLIENT-234761");

await jira.InvokeTransitionAsync(reference, "242151256", null, cancellationToken);
```

## Mapping классы

Для более удобной работы с Jira рекомендуется использовать Mapping классы. Рекомендую использовать отдельный класс для каждого issue type, определённого в Jira.

Для каждого маппинг класса необходимо объявить 2 конструктора. А также необходимо объявить атрибут JiraIssuePropertyAttribute для каждого поля. Не забываем, что Mapping класс должен наследоваться от класса JiraIssue 

```c#
public class JiraClientIssue : JiraIssue {
    
    public JiraClientIssue()
    {
    }

    public JiraClientIssue(IJiraIssueFieldsController controller) 
        : base(controller)
    {
    }

    // Для стандартного поля Jira
    [JiraIssueProperty("updated")]
    public DateTime Updated
    {
        get => Controller.GetValue<DateTime>("updated");
        set => Controller.SetValue("updated", value);
    }

    // Для customField
    [JiraIssueProperty(11323)]
    public string AwesomeCustomField
    {
        get => CustomFields[11323].Get<string>();
        set => CustomFields[11323].Set(value);
    }
}
```

Маппинг классы позволяют не держать в уме идентификаторы для customField, а работать с полями в Jira с помощью естественных метафор.

```c#
JiraIssueReference referene = JiraIssueReference.FromKey("KNOPKLIENT-234761");
JiraClientIssue issue = new JiraClientIssue {
    AwesomeCustomField = "Мое чудесное поле"
};

await jira.UpdateIssueAsync(reference, issue, cancellationToken);
```

#### Scope

Обычно Mapping класс используется для каждого issue type. Однако issue type часто накладывает ограничения на issue такие как 'project' (проект) и 'issueType' (тип issue). В частности, для того чтобы создать в Jira новый issue необходимо всегда указывать необходимые поля. Это может быть утомительно и об этом факте легко забыть, поэтому рекомендую для удобства объявлять Scope в Mapping классе.

```c#
public class JiraClientIssue : JiraIssue {

    // конструкторы и поля

    private class Scope : IDefineScope<JiraClientIssue>
    {
        public void Build(IScopeBuilder<JiraClientIssue> builder)
        {
            builder
                .Define(x => x.Project, new JiraProject { Key = "KNOPKLIENT"})
                .Define(x => x.IssueType, new JiraIssueType { Id = "22", Name = "Клиент" });
        }
    }
}
```   

Таким образом, при создании экземпляра класса необходимые поля будут заполнены.

```c#
JiraClientIssue issue = new JiraClientIssue();

Console.WriteLine(issue.Project.Key);    // KNOPKLIENT
Console.WriteLine(issue.IssueType.Name); // Клиент 
```

Кроме того, при работе с Jira через Linq-Provider на запрос в Jira автоматически наложится фильтр "project = KNOPKLIENT AND issueType = Клиент", что позволит Jira выполнить запрос более эффективно. 

ВАЖНО: При выполнении метода IJira.SelectIssuesAsync дополнительные фильтры на JQL не наложатся.

## InMemory реализация

При автоматическом тестировании кода, взаимодействующего с Jira, обычно нет возможности поднять тестовый instance обычной Jira. Поэтому можно использовать InMemory эмулятор Jira. И даже если этот instance будет поднят, то это может вызывать следующие проблемы для автоматического тестирования.

- Скорость. Настоящая Jira работает медленно, что неприемлемо для работы в стиле TDD, при котором крайне важно получать мнговенную обратную связь от тестов;
- Создание тестовой среды. Обычно для тестов необходимо создать только минимально необходимый пресет данных в Jira. В настоящей Jira чаще всего необходимо создавать полный пресет данных. Таким образом код теста становится перенасыщенным и перестаёт отражать суть тестируемого взаимодействия. В InMemory реализации нет дополнительных проверок на обязательность заполнения полей, которые неважны для теста.
- Независимость тестов. При использовании настоящей Jira в тестах необходимо обеспечить изоляцию тестов, в которой состояние Jira после каждого теста будет скидываться к начальному состоянию. В базах данных это может достигаться транзакциями, которых в Jira нет. InMemory реализацию можно создавать одну на каждый тест, что обеспечит пустую Jira и избавит от нежелательных side-эффектов и морганий.

#### Инициализация InMemory

```c#
JiraUser currentUser = new JiraUser {
    Key = "coolwage" 
};
JiraMetadataProvider metadata = new JiraMetadataProvider(new [] {typeof(JiraClientIssue)});

IJira jira = FakeJira.InMemory("http://fake.jira.int", currentUser, metadata);
```

## Файловая реализация

Для интеграционного тестирования необходимо, чтобы разные сервисы системы, развёрнутые в разных процессах имели доступ к одному и тому же экземпляру Jira. Дле этого создана файловая реализация эмулятора

#### Инициализация файловой реализации

```c#
var folderPath = Path.Combine(Path.GetTempPath(), "fileJiraImplementation");
JiraUser currentUser = new JiraUser {
    Key = "coolwage" 
};
JiraMetadataProvider metadata = new JiraMetadataProvider(new [] {typeof(JiraClientIssue)});

IJira jira = FakeJira.File(folderPath, "http://fake.jira.int", currentUser, metadata);
```


## Linq-Provider

Linq-Provider естественный инструмент в .NET среде, позволяющий работать с базами данных и любыми другими провайдерами данных через интерфейс Linq при наличии Object-Relational Mapping. Роль последнего в данном случае выполняет множество Mapping классов.

Для инициализации Linq-Provider необходимо создать класс JiraMetadataProvider. Который в конструкторе принимает коллекцию из всех типов Mapping классов, объявленных в проекте.

Инициализация Linq-Provider

```c#
JiraMetadataProvider metadata = new JiraMetadataProvider(new [] {typeof(JiraClientIssue)});
IJira jira = Jira.RestApi("http://myjira.com", "my_user", "my_password");

JiraQueryProvider provider = new JiraQueryProvider(jira, metadata);
``` 

Инициализация InMemory Linq-Provider

```c#
JiraUser currentUser = new JiraUser {
    Key = "coolwage" 
};
JiraMetadataProvider metadata = new JiraMetadataProvider(new [] {typeof(JiraClientIssue)});
IJira jira = FakeJira.InMemory("http://fake.jira.int", currentUser, metadata);

JiraQueryProvider provider = new JiraQueryProvider(jira, metadata);
```

###Сценарии

#### Where

```c#
JiraClientIssue[] issues = provider.GetIssues<JiraClientIssue>()
    .Where(x => x.Assignee == "coolwage")
    .ToArray();
```

Поддерживаемые операции

- x => x.Assignee == "coolwage", транслируется в "assignee = coolwage"
- x => x.DueDate = DateTime.Now, транслируется в "duedate = '2020-12-08 00:00'"
- x => JqlFunctions.Contains(x.Summary, "тест"), транслируется в "summary ~ тест"
- x => x.Labels.Contains("mylabel"), транслируется в "labels = mylabel"
- x => x.DueDate [>, <, >=, <=] DateTime.Now, транслируется в ""duedate [>, <, >=, <=] '2020-12-08 00:00'"" 

#### Select

Если не указать в запросе конструкцию Select, то Linq-Provider запросит у Jira информацию по всем полям, которые есть в Jira для текущего issue type. В случае когда необходимо выбрать большое количество issue — Jira может быть серьёзно нагружена и запрос к ней упадёт по таймауту. Поэтому rest api требует указывать поля, необходимые к выдаче для запроса. Для поддержки этого механизма в Linq-Provider включена поддержка конструкции Select.

Данный запрос вернёт summary всех issue, которые имеют issueType, определённый в Scope у Mapping класса JiraClientIssue.

```c#
string[] issues = provider.GetIssues<JiraClientIssue>()
    .Select(x => x.Summary)
    .ToArray();
``` 

В Jira уйдёт следующий запрос. Обратите внимание, что jql в данном случае будет не пустым, если в JiraClientIssue объявлен Scope

```json
{
  "jql": "project = KNOPKLIENT AND issueType = Клиент",
  "fields": ["summary"],
  "startAt": 0,
  "maxResults": 200
}
```

Поддерживаются следующие конструкции:

- единственное поле Select(x => x.Summary);
- анонимные типы Select(x => new { Text = x.Summary });
- обычные классы с инициализаций properties Select(x => new MyAwesomeClass {Text = x.Summary});

#### Contains

Аналог IN фильтра в JQL

```c#
JiraProject project = new JiraProject {
    Key = "KNOPKLIENT"
};
string[] issues = provider.GetIssues<JiraClientIssue>()
    .Where(x => new[] {project}.Contains(x.Project))
    .ToArray();
``` 

Будет транслирован в 

```json
{
  "jql": "project in (KNOPKLIENT)",
  "startAt": 0,
  "maxResults": 200
}
```

#### Count

Получение количества issue. При этом запрос не будет материализовывать все issue, он только запросит у Jira информацию о количестве issue

```c#
int count = provider.GetIssues<JiraClientIssue>()
    .Count();
``` 


#### Any

Получение информации о налиции issue, удовлетворяющих фильтру. При этом запрос не будет материализовывать все issue, он только запросит у Jira информацию о количестве issue

```c#
bool exists = provider.GetIssues<JiraClientIssue>()
    .Any();
``` 

#### Подзапросы

В Jira реализован механизм issueFunction, который позволяет выполнить подзапрос. В данном провайдере реализовано только два типа подзапросов, соответсвующие функциям parentsOf и subtasksOf

#### ParentsOf

```c#
JiraIssues[] issues = provider.Select<JiraIssue>()
    .Where(issue => provider.Select<JiraClientIssue>()
                        .Where(parent => parent.Assignee == "coolwage")
                        .Any(parent => issue.Parent == parent))
    .ToArray();
```

Будет транслирован в 

```json
{
  "jql": "issueFunction in parentsOf(\"assignee = coolwage\")",
  "startAt": 0,
  "maxResults": 200
}
```

#### SubtasksOf
```c#
JiraClientIssue[] issues = provider.Select<JiraClientIssue>()
    .Where(issue => provider.Select<JiraIssue>()
                        .Where(child => child.Assignee == "coolwage")
                        .Any(child => child.Parent == issue))
    .ToArray();
```

Будет транслирован в 

```json
{
  "jql": "issueFunction in subtasksOf(\"assignee = coolwage\")",
  "startAt": 0,
  "maxResults": 200
}
```
