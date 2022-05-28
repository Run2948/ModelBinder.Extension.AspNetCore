# ModelBinder.Extension.AspNetCore

A simple parameter binding component for ASP.NET Core Web and Web API application.

## 快速使用

### 1. 安装

* [ModelBinder.Extension.AspNetCore](https://www.nuget.org/packages/ModelBinder.Extension.AspNetCore)

``` bash
dotnet add package ModelBinder.Extension.AspNetCore
```


### 2. 注册

在 `Startup` 文件中注册组件，参考 [Startup.cs](https://github.com/Run2948/ModelBinder.Extension.AspNetCore/blob/master/samples/ModelBinderDemo/Startup.cs)

- 使用  `FromJsonBody`

```csharp
// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    ...
        
    app.UseRouting();

    app.UseAuthorization();

    app.UseFromJsonBody(); // should be added before UseEndpoints

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}
```

- 使用  `FromSmartBody`

```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    });
    
	// 方式一：
    //services.AddControllers(options =>
    //{
    //    options.ModelBinderProviders.InsertSmartBodyBinding();
    //});

    // 方式二：
    services.AddControllers().AddSmartBody();
}
```


### 3. 使用

#### 3.2 FromJsonBody


示例代码： [ValuesController.cs](https://github.com/Run2948/ModelBinder.Extension.AspNetCore/blob/master/samples/ModelBinderDemo/Controllers/ValuesController.cs)


``` csharp
[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    [HttpGet]
    [HttpPost]
    public int Post([FromJsonBody("i1")] int i3, [FromJsonBody] int i2, [FromJsonBody("author.age")] int aAge, [FromJsonBody("author.father.name")] string dadName)
    {
        System.Diagnostics.Debug.WriteLine(aAge);
        System.Diagnostics.Debug.WriteLine(dadName);
        return i3 + i2 + aAge;
    }
}
```

Postman 请求：

```http
curl --location --request POST 'http://localhost:19623/api/values' \
--header 'Content-Type: application/json' \
--data-raw '{
    "i1":3,
    "i2":2,
    "author": {
        "age": 26,
        "father": {
            "name": "John"
        }
    }
}'
```



#### 3.2 FromSmartBody

示例代码： [Values2Controllercs](https://github.com/Run2948/ModelBinder.Extension.AspNetCore/blob/master/samples/ModelBinderDemo/Controllers/Values2Controller.cs)


``` csharp
[Route("api/[controller]")]
[ApiController]
public class Values2Controller : ControllerBase
{
    [HttpPost]
    public string Post([FromSmartBody("id")] int dadId, [FromSmartBody] string dadName)
    {
        System.Diagnostics.Debug.WriteLine(dadId);
        System.Diagnostics.Debug.WriteLine(dadName);
        return $"{dadId}:{dadName}";
    }
}
```

Postman 请求：

From query  `params`

```http
curl --location --request POST 'http://localhost:19623/api/values2?id= 3&dadName= 爸爸'
```

From body  `form-data`:

```http
curl --location --request POST 'http://localhost:19623/api/values2' \
--form 'id=" 3"' \
--form 'dadName=" 爸爸"'
```

From body  `x-www-form-urlencoded`:

```http
curl --location --request POST 'http://localhost:19623/api/values2' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'id= 3' \
--data-urlencode 'dadName= 爸爸'
```

From body  `raw`:

```http
curl --location --request POST 'http://localhost:19623/api/values2' \
--header 'Content-Type: application/json' \
--data-raw '{
    "id": 3,
    "dadName": "爸爸"
}'
```

##### Binding arrays in query parameters

```csharp
[Route("api/[controller]")]
[ApiController]
public class Values3Controller : ControllerBase
{
    [HttpGet]
    public string Get([FromSmartBody("dadIds")] int[] ids, [FromSmartBody] IEnumerable<string> hobbies)
    {
        System.Diagnostics.Debug.WriteLine(ids);
        System.Diagnostics.Debug.WriteLine(hobbies);
        return $"{string.Join(",", ids)}，爱好：{string.Join("、",hobbies)}";
    }
}
```

Postman 请求：

```http
curl --location -g --request GET 'http://localhost:19623/api/Values3?dadIds=[1,2,3]&hobbies=["篮球“,"足球","排球"]'
```

### 4. 鸣谢

  感谢两位 .Net 大佬提供的开源技术和设计思路:

* **ldqk**  -  [Masuit.Tools.AspNetCore](https://github.com/ldqk/Masuit.Tools/tree/master/Masuit.Tools.AspNetCore)

* **yangzhongke**  -  [YouZack.FromJsonBody](https://github.com/yangzhongke/YouZack.FromJsonBody)
