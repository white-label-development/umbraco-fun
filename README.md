# umbraco-fun

learning umbraco...

tutorials are all a bit basic - but I am a beginner so lets see how this goes.

## snippets - mostly regarding data access, pulled from Umraco.TV

IPublishedContent is the standard model used for all published content.

```
int Id { get; }
…
IPublishedContent Parent { get; }
IEnumerable<IPublishedContent> Children { get; }
```



```
var rootNodes = Umbraco.TypedContentAtRoot();
var homeNodeById = rootNodes.First(x => x.Id == 1077); //by node id
var rootNodes.FirstOrDefault(x => x.DocumentTypeAlias == "myHomePage"); //by dt alias

```

`var contentModel = Model.Content.As<GameGatewayModel>(); //cast model content to our model`

`var spotModel = Model.Content.GetPropertyValue<SpotModel>("spotGame"); // "raw" umbraco IPublishedContent Id, Parent, Children etc`

where .Properties is a coll of `IPublishedProperty`

In Um there is insert dialogue that can be used to write a lot of template code for you. Check this out for syntax basics.

```
Umbraco.TypedContentAtRoot(); //will return a collection of all nodes in the root of your content tree, irrespective of path and tree structure

var siteSettings= Umbraco.TypedContentAtRoot().FirstOrDefault(x => x.ContentType.Alias.Equals("SiteSettings")); 
```

#### tv: Querying Umbraco data with Razor

If you are working in a custom MVC Controller's action, a model of type `RenderModel` will be provided in the Action's method parameters. This model contains an instance of `IPublishedContent` which you can use.

When you are working in a View of type `UmbracoTemplatePage` (which is the default view type), the Model provided to that view will also be `RenderModel` (which exposes IPublishedContent).

All Umbraco view page types inherit from `UmbracoViewPage<TModel>`. A neat trick is that if you want your view Model to be `IPublishedContent` you can change your view type to `UmbracoViewPage<IPublishedContent>` and the view will still render without issue even though the controller is passing it a model of type `RenderModel`.

`@Umbraco.Field("promoTitle")`
This seems to be the standard way to get data entered into a DocumentType (page) using the @Umbraco helper. 
To clarify, we are on Blob Post "About Bob" and getting the information we entered into that particular post - not data elsewhere.

```var selection = Model.Content.Site().Children().Where(x => x.IsVisible())
@Item.Url, @Item.Name
// Although this looks like we are traversing the properties of a model I guess we must also be creating a query here
// in this case from the site root get all visible children. Presumably this doesn't actually fetch the entire db, just one level?

//from the site root, get blogpage's blogitems
var selection = Model.Content.Site().FirstChild("blogPage").Children("blogItem").Where(x => x.IsVisible())
```

There is an Umbraco "convention" where umbracoNaviHide and Visible can be used to hide by conventions.

Needs a property with the alias umbracoNaviHide on the DocumentType > Generic Properties, as TrueFalse Type this allows the
.Where("Visible") bit. This is different from `x => x.IsVisible` which is full hiding, not just from nav


```
// debug helper
@if (Model != null && Model.Properties.Any())
{
    <ul>
        @foreach (var property in Model.Properties.OrderBy(x => x.PropertyTypeAlias))
        {
            <li>@($"{property.PropertyTypeAlias}: {property.Value}")</li>
        }
    </ul>
}
```

`string imageUrl = Model.Content.GetPropertyValue<IPublishedContent>("headerImage").Url; //img url `

`CurrentPage.Children  //Listing SubPages rfom Current Page`

```//Listing SubPages by Level (eg: always show level 2 items as nav (home = 1)
.AncestorOrSelf(level) // returns DynamicPublishedContect can then get .Children()
```

`.isAncestorOrSelf(CurrentPage) ? "myActiveCssClass" : null //set active nav item snippet`


#### Macros (templates, RTE, Grid) and Partial (templates) Views

note: partial view macro file is the display(?) of the macro, not the macro itself.

macros can be configured with parameters and easy to enable caching.
 
`@Umbraco.RenderMacro("alias"); //macro no params `

`Model.GetParameterValue<int>("alias", defaultValue)`

#### Models Builder

a tool that generates strongly types models based upon Document Types. Since 7.4

Generates model to memory (on startup?)

see appSettings .Enable and .ModelsMode (PureLive ||  Dll || AppData || EnableApi || LiveDll || LiveAppData)

PureLive - runtime generation - for Um backoffice template use.

Dll - compiles to /bin/Umbraco.Web.PublishedContentModels.dll and restarts the application - for VS use. rm dll if switching to another mode.

AppData - models generated in /app_data/Models. Requires include in VS. Restarts the app. For VS use, extend, inherit etc. rm if switching.

API Models - use an API to expose models, requires VS extension (and Models builder api via Nuget). For CS use, extend, inherit and split out into a separate project. The API is only provided in debug mode. Also in appSettings .ModelsMode="", not ="EnableApi" , weirdly.





Models builder replaces Dynamic Published Content support (droped in v8). `CurrentPage.Property` etc. Probs because dynamics are a pain.
The IPublishedContent style was ` Model.Content.GetPropertyValue<T>` @CurrentPage and the UmbracoHelper query methods like @Umbraco.Content or @Umbraco.Media instead of the typed methods like @Umbraco.TypedContent and @Umbraco.TypedMedia show you are using dynamics

Models builder stylee `@Model.BodyText` is strongly typed.

If the .cshtml inherits from `UmbracoTemplatePage<T>` we can access DynamicPublishedContent and IPublishedContent.
In reality the actual class name for T will be underlined by VS, because in the native "PureLive" mode the model is generated at runtime, so currently does not exist.

```
var x = CurrentPage.ArticlePublishDate; // DPC returns a dynamic object = meh
var y = Model.Content.GetPropertyValue<DateTime>("articlePublishDate");
vay z = Model.Content.ArticlePublishDate; // new hotness via inherts of UmbracoTemplatePage<ContentModel.NewsItem>   
```

When we create a template (in the back office) Umbraco will add `@inherits Umbraco.Web.Mvc.UmbracoTemplatePage` which gives us
access to publsihed content models, we can traverse the tree etc.

When we create a DocumentType with an associated template, the template also has the `UmbracoTemplatePage<T>` and
`using ContentModels = Umbraco.Web.PublishedContentModels;'.

With UmbracoTemplatePage<T>, @Model.Content is an instance of T. Also exposes DynamicPublishedContent.


In v8 this is `UmbracoViewPage<T>` and @Model is an instance of T





















### Pipeline (from the v7 docs)

The request pipeline is the process of building up the URL for a node, resolving a request to a specified node and making sure that the right content is sent back.

Inbound is every request received by the web server and handled by Umbraco.. The inbound process is triggered by the Umbraco (http) Module. The published content request preparation process kicks in to create an PublishedContentRequest instance.

It is called in `UmbracoModule.ProcessRequest(…)`

What it does:

+ It ensures Umbraco is ready, and the request is a document request.
+ Creates a PublishedContentRequest instance
+ Runs PublishedContentRequestEngine.PrepareRequest() for that instance
+ Handles redirects and status
+ Forwards missing content to 404
+ Forwards to either MVC or WebForms handlers

Once the request is prepared, an instance of PublishedContentRequest is available which represents the request that Umbraco must handle. It contains everything that will be needed to render it

Umbraco runs all content finders, stops at the first one that returns true.
Finder can set content, template, redirect… eg:

```
public class MyContentFinder : IContentFinder
{
    public bool TryFindContent(PublishedContentRequest request)
    {
        var path = request.Uri.GetAbsolutePathDecoded();
        if (!path.StartsWith("/woot"))
        return false; // not found
        
        // have we got a node with ID 1234?
        var contentCache = UmbracoContext.Current.ContentCache;
        var content = contentCache.GetById(1234);
        if (content == null) return false; // not found
     
        // render that node
        request.PublishedContent = content;
        return true;
    }
}
```

Unless we are hihacking a route, everything then goes to `Umbraco.Web.Mvc.RenderMvcController` `Index()`

`Something something = SomethingResolver.Curent.Something; // uses object resolvers to get interface implementations`

Resolvers are initialised when the app starts, then resolution is frozen.

##### Application Event Handler

Umbraco will find and run every implementation of `IApplicationEventHandler`. 
Better to inherit from the abstract class, and override only what’s needed.

`void OnApplicationInitialised` = ApplicationContext Exists.

`void OnApplicationStarting` = resolvers initialized, resolution not frozen

`void OnApplicationStarted` = Boot completed, resolution is frozen.


```
public class MyApplication : ApplicationEventHandler
{
    protected override void ApplicationStarting(…)
    {
        SomethingResolver.Current.SetSomething(new SomethingBetter());
        SomethingsResolver.Current.RemoveType<Something>();
        SomethingsResolver.Current.AddType<SomethingBetter>();
    }
}
```
^^ Drop that class anywhere in your code and you're done configuring the resolver.

##### User Request > Request Pipeline 
**Inbound request pipeline (match url to a content item and determine rendering engine)**

**Controller selection (match contoller+action to request)**

##### Execute request (MVC action+view are executed. Can query for published data)
**IPublishedContent**

**DynamicPubiishedContent (avoid as dropped in 8, but used in Zeit)**

**Umbraco Helper (use to query published media and content)**

**Members (MembershipHelper)**




##### Outbound pipeline

Outbound is the process of building up a URL for a requested node.

Creates segments: “our-products”, “swibble”…

Creates paths:  “/our-products/swibble”

Creates urls: “http://site.com/our-products/swibble.aspx”

Each published content has a url segment, a.k.a. “urlName”.


### Links

#### the most important umbraco link ever is this:
https://our.umbraco.com/documentation/reference/common-pitfalls/




#### quickly snagged these off codeshare
https://codeshare.co.uk/blog/how-i-use-source-control-for-my-umbraco-website-source-code-and-media/

https://codeshare.co.uk/blog/how-to-set-the-default-page-base-type-to-umbracoviewpage-in-umbraco/

https://codeshare.co.uk/blog/xml-sitemap-in-umbraco-website/

https://codeshare.co.uk/blog/how-to-search-by-document-type-and-property-in-umbraco/

https://codeshare.co.uk/blog/how-to-find-the-home-page-in-umbraco/

https://codeshare.co.uk/blog/umbraco-related-links-into-c-class-objects/

https://codeshare.co.uk/blog/how-to-use-an-umbraco-data-type-to-populate-an-mvc-drop-down-list/

https://codeshare.co.uk/blog/how-to-create-a-carousel-in-umbraco-using-nested-content-and-bootstrap/

https://codeshare.co.uk/blog/how-to-use-donut-caching-in-umbraco-and-mvc/

https://codeshare.co.uk/blog/how-to-get-the-picked-item-name-in-stacked-content-and-nested-content-using-ncnodename/

https://codeshare.co.uk/blog/how-to-include-scripts-from-partial-views-in-mvc-and-umbraco/

#### others

https://www.jondjones.com/learn-umbraco-cms/umbraco-7-tutorials/

https://www.jondjones.com/learn-umbraco-cms/umbraco-7-tutorials/umbraco-deployment/how-to-configure-umbraco-to-work-in-a-load-balanced-environment/

https://farmcode.org/articles/how-to-get-umbraco-root-node-by-document-type-in-razor-using-umbraco-7-helper/

https://skrift.io/articles/archive/testing-the-performance-of-querying-umbraco/

https://our.umbraco.com/documentation/Reference/Routing/Request-Pipeline/document/TheUmbracoRequestPipeline.pdf

