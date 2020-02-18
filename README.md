# umbraco-fun

learning umbraco....


tutorials are all a bit basic - but I am a beginner so lets see how this goes. Created the David Hasselhof tribute site as a simple work area, and treven.co.uk as a real world, slightly overblown (could be simpler, but then I'd miss learning opportunities)

xxxx.localtest.me > 127.0.0.1 trick is new (not Um specific)

### "Implementor tips" / Different DocType usages

Create pages, but without direct 'properties'. Instead, create Compositions and apply these to the pages.
eg:New Compostion > Document Type without a Template > "Visibility Controls" > add umbracoNaviHide > TrueFalse. add hideFromXmlMap > true false. 

Other compositions might be "Content Controls", "Article Controls", "Header Controls". Of note: "Contact Form Controls" - which contains entries for a SucessMessage and a FailureMessage. Good idea!

Use of Elements for icons, for use in nested content etc.

To reiterate (with help from Moriyama) broadly speaking there are: 

+ ‘Compositions’ - used to group properties together and reuse them in the construction of other Document Types
+ ‘Elements’ - these new smaller schema configurations for defining repeating items in Nested Content
+ ‘Folders’ - I suggest organising ‘each’ type of Document Type into separate folders

 And Document Types themselves for creating Content Items which break down into:

+ ‘Pages’ - the pages of the site, document types with defined templates - these pages have ‘Urls’
+ ‘Components’ - items in the content tree without a template, they have no external Url - often to be ‘picked’ to share content across multiple pages. NT: and often a datastore, like a row in a table

NB: ‘Components’ here isn’t a proper ‘Umbraco term’ - some people call them widgets, or cogs or resources, pods, or blocks etc etc, and you start to see how not having a common domain language for things creates confusion…

## snippets - mostly regarding data access.

IPublishedContent is the standard model used for all published content.

For read operation use `UmbracoHelper` to access the `PublishedContentQuery` methods as these operate against a cache of published content items, and are significantly quicker.

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

```
var title = Model.HasValue("title") ? Model.Value("title") : Model.Name;
var image = Model.Value<IPublishedContent>("myImage"); // also ,fallback: Fallback.ToAncestors to look up if null.
string imageUrl = imaage.GetCropUrl(800, 600);
```

`var contentModel = Model.Content.As<GameGatewayModel>(); //cast model content to our model`

`var spotModel = Model.Content.GetPropertyValue<SpotModel>("spotGame"); // "raw" umbraco IPublishedContent Id, Parent, Children etc`

where .Properties is a coll of `IPublishedProperty`

In Um there is insert dialogue that can be used to write a lot of template code for you. Check this out for syntax basics.

```
Umbraco.TypedContentAtRoot(); //will return a collection of all nodes in the root of your content tree, irrespective of path and tree structure

var siteSettings= Umbraco.TypedContentAtRoot().FirstOrDefault(x => x.ContentType.Alias.Equals("SiteSettings")); 
```

https://our.umbraco.com/Documentation/Reference/Querying/IPublishedContent/Collections/index-v7

UDI's from 7.6+ eg: `umb://document/4fed18d8c5e34d5e88cfff3a5b457bf2`


#### DI

setup via `OnApplicationStarted` or `Umbraco.Web.IApplicationEventHandler`.

`builder.RegisterType<MyAwesomeContext>(); // add custom class to the container as Transient instance`


Can make use of the underlying DI framework to create custom Services and Helpers, that in turn can have the 'core' management Services and Umbraco Helpers injected into them.

Note: choose what to inject: `IPublishedContentQuery ` (has an UmbracoContext) vs `IUmbracoContextFactory + EnsureUmbracoContext()` (no UmbracoContext, eg: an event hamdler) (you should NEVER have to inject the UmbracoContext itself directly into any of your constructors)

To access the service directly from the view you would need to use the Service Locator pattern and the Current.Factory.GetInstance() method to get a reference to the concrete implementation of the service registered with DI:
```
ISiteService SiteService = Current.Factory.GetInstance<ISiteService>();
IPublishedContent newsSection = SiteService.GetNewsSection();
```


Register the custom service with Umbraco's underlying DI container using an IUserComposer:
```
public class RegisterSiteServiceComposer : IUserComposer
{
    public void Compose(Composition composition)
    {
        // if your service makes use of the current UmbracoContext, eg AssignedContentItem - register the service in Request scope
        // composition.Register<ISiteService, SiteService>(Lifetime.Request);
        // if not then it is better to register in 'Singleton' Scope
        composition.Register<ISiteService, SiteService>(Lifetime.Singleton);
    }
}
```
Because we've registered the SiteService withthe DI framework we can inject the service into our controller's constructor, in the same way as 'core' Services and Helpers.
```
public BlogPostController(ISiteService siteService)
{
    _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
}

//or more correctly use the following example instead which supplies all constructor parameters for the base class.
public BlogPostController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, UmbracoHelper umbracoHelper, ISiteService siteService)
            : base(globalSettings, umbracoContextAccessor, services, appCaches, profilingLogger, umbracoHelper)
{
    _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
}
```




#### Service Context

The Umbraco Services layer is used to query and manipulate Umbraco stored in the database. Exposed as a property on all Umbraco base classes such as SurfaceControllers, UmbracoApiControllers, any Umbraco views, etc. eg: `Services.ContentService.Get(123);`.

If you are not working with an Umbraco base class and the ServiceContext is not exposed, you can access the ServiceContext via the ApplicationContext. Like the ServiceContext, the ApplicationContext is exposed an all Umbraco base classes, but in the rare case that you are not using an Umbraco base class, you can access the ApplicationContext via a singleton. For example:
```
ApplicationContext.Current.Services.ContentService.Get(123); // do not use this is a view/template, see below
```

"Although there is a management Service named the ContentService - only use this to modify content - do not use the ContentService in a View/Template to pull back data to display, this will make requests to the database and be slow - here instead use the generically named UmbracoHelper to access the PublishedContentQuery methods that operate against a cache of published content items, and are significantly quicker."

```
 IPublishedContent publishedContentItem = Umbraco.Content(123); // from a template/view (that :UmbracoViewPage) retrieve an item from Umbraco's published cache with id 123
```
^^ same of a custom Contoller. Both can use `Services`. Both have an Umbraco context.

Components, ContentFinders, Custom C# clases may not have the Umbraco context.

UmbracoContext, UmbracoHelper, PublishedContentQuery - are all based on an HttpRequest - their lifetime is controlled by an HttpRequest. So if you are not operating within an actual request, you cannot inject these parameters and if you try to ... Umbraco will report a 'boot' error on startup. However, there is a technique that allows the querying of the Umbraco Published Content, using the `UmbracoContextFactory` and calling `EnsureUmbracoContext()`.

It's possible to inject management Services that do not rely on the UmbracoContext into the constructor of a component. 

```
public SubscribeToContentSavedEventComponent(IMediaService mediaService) { ... } //see Composing for more...
```

#### Surface Controllers

Are MVC controllers (inherit from Umraco.Web.Mvc.SurfaceController) that interact with the front end rendering of an Umbraco page.

Used to render Child Action content and handle form data submission. Has access to Umbraco helper methods and properties (via inheritance).

```
@Html.Action("Index", "TempExampleSurface")
```

To use surface controllers in the RTE use a macro.


#### Content API

`ContentService` is the gateway to the Content API. If using within a `SurfaceController`, is available using `.Services`

scenario: front end 'add comment' form posts to a SurfaceController. We want to save the content.

Using `Services.ContentService.CreateContent()`
we need: Name (for new Content), ParentId, DocumentTypeAlias (for new Content). 

```
var newComment = Services.ContentService.CreateContent("my new Comment", CurrentPage.Id, "Comment");
newComment.setValue("name", model.Name);
...
Services.ContentServices.SaveAndPublish(newComment);


```

#### Querying Umbraco data (across time 'cos it changes a lot in different versions)

If you are working in a custom MVC Controller's action, a model of type `RenderModel` will be provided in the Action's method parameters. This model contains an instance of `IPublishedContent` which you can use.

When you are working in a View of type `UmbracoTemplatePage` (which is the default view type), the Model provided to that view will also be `RenderModel` (which exposes IPublishedContent).

All Umbraco view page types inherit from `UmbracoViewPage<TModel>`. A neat trick is that if you want your view Model to be `IPublishedContent` you can change your view type to `UmbracoViewPage<IPublishedContent>` and the view will still render without issue even though the controller is passing it a model of type `RenderModel`.

`@Umbraco.Field("promoTitle")`
This seems to be the standard way to get data entered into a DocumentType (page) using the @Umbraco helper. 
To clarify, we are on Blob Post "About Bob" and getting the information we entered into that particular post - not data elsewhere.

With an older Umbraco implementation you might find `Model.Content.GetPropertyValue<string>(“subTitle”)` to write out a property value in a template, or `Umbraco.Field(“subTitle”)`. In V8 the syntax has become `Model.Value<string>(“subTitle”)`


```var selection = Model.Content.Site().Children().Where(x => x.IsVisible())
@Item.Url, @Item.Name
// Although this looks like we are traversing the properties of a model I guess we must also be creating a query here
// in this case from the site root get all visible children. Presumably this doesn't actually fetch the entire db, just one level?

//from the site root, get blogpage's blogitems
var selection = Model.Content.Site().FirstChild("blogPage").Children("blogItem").Where(x => x.IsVisible())
```

There is an Umbraco "convention" where umbracoNaviHide and Visible can be used to hide by conventions.

Needs a property with the alias umbracoNaviHide on the DocumentType > Generic Properties, as TrueFalse Type this allows the
.Where("Visible") bit. This is different from `x => x.IsVisible` which is full hiding, not just from nav EDIT: x => x.IsVisible() does seem to relate to umbracoNaviHide.


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



```
CurrentPage.Children  //Listing SubPages rfom Current Page
CurrentPage.Ancestors //Returns all ancestors of the current page (parent page, grandparent and so on)
CurrentPage.Ancestor //Returns the first ancestor of the current page
CurrentPage.Descendants //Returns all descendants of the current page (children, grandchildren etc)
CurrentPage..DescendantsOrSelf //Returns all descendants of the current page (children, grandchildren etc), and the current page itself


//Listing SubPages by Level (eg: always show level 2 items as nav (home = 1)
.AncestorOrSelf(level) // returns DynamicPublishedContect can then get .Children()


@foreach(var item in Model.Content.DescendantsOrSelf().OfTypes("widget1", "widget2")) { ... }

var nodes = Model.Content.Children.OrderBy(x => x.GetPropertyValue<string>("title"))
```

`.isAncestorOrSelf(CurrentPage) ? "myActiveCssClass" : null //set active nav item snippet`

**Templates** are used for the HTML layout of your pages. **Partials** can be included in your templates for shared functionality across different page templates. **Macros** can be used for reusable dynamic components that can be controlled by editors to embed functionality into the grid or rich text areas.


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


### Events

Subscribe when Application starts, so will inherit ApplicationEventHandler and override ApplicationStarted.

Can find events with intellisense, eg: Umbraco.Core.Servcies.ContentService. ... Publishing, Published etc

`Umbraco.Core.Servcies.ContentService.published += MyContentService_Published;`

`e.Cancel = true; //cancel in a "before" event, such as Publishing.`

### Compsing replaces? Events, or some of them

"What's the benefir of compsong? For example when you wanted to change how things where composed (register a new finder etc) you would have to remember to do it in the proper “event” – and people were always confused – now there’s the Composemethod explicitly for this usage – same for Initialize, more explicit + manages dependencies & injection

Also components can depend on each other and this ensures they run one after another, whereas ApplicationEventHandlers were in random order and are easy to enable/disable components, which could not be done with app handlers.

oh and components also terminate meaning they are notified when Umbraco stops

so you create a component, and in Compose you tell Umbraco that it should use MyCache as a content cache. In Initialize you load your cache from wherever you want. In Terminate you flush changes to disk."


### "Composing" (Customising the behaviour at 'start up'. e.g. adding, removing or replacing the core functionality of Umbraco or registering custom code to subscribe to events.)

An Umbraco application is a Composition made of many different 'collections' and single items of specific functionality/implementation logic/components (eg. UrlProviders, ContentFinders etc). These collections are populated when the Umbraco Application starts up.

'Composing' is the term used to describe the process of curating which pieces of functionality should be included in a particular collection. The code that implements these choices at start up is called a `Composer`.

How are the collections populated? - Either by scanning the codebase for c# classes that inherit from a particular base class or implement a particular interface (typed scanned) or by being explicitly registered via a Composer.

A `Component` is a generic wrapper for writing custom code during composition, it has two methods: `Initialize()` and `Terminate()` and these are executed when the Umbraco Application starts up, and when it shuts down, respectively. Typically a Component may be used to wire up custom code to handle a particular event in Umbraco. see https://our.umbraco.com/documentation/Implementation/Composing/

Umbraco ships with a set of ICoreComposer's that pull together the default set of components and collections that deliver the core 'out of the box' Umbraco behaviour. See `IUserComposer`'s (for developers to use) and `IComponent`.

A collection builder builds a collection, allowing users to add and remove types before anything is registered into DI.

see `UmbracoComponentBase, IUmbracoUserComponent`

### Pipeline (from the v7 docs)

The request pipeline is the process of building up the URL for a node, resolving a request to a specified node and making sure that the right content is sent back.


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

Inbound is every request received by the web server and handled by Umbraco.. The inbound process is triggered by the Umbraco (http) Module. The published content request preparation process kicks in to create an `PublishedContentRequest` instance.

It is called in `UmbracoModule.ProcessRequest(…)`

What it does:

+ It ensures Umbraco is ready, and the request is a document request.
+ Creates a PublishedContentRequest instance
+ Runs PublishedContentRequestEngine.PrepareRequest() for that instance
+ Handles redirects and status
+ Forwards missing content to 404 (with the last chance IContentFinder I think)
+ Forwards to either MVC or WebForms handlers

Once the request is prepared, an instance of `PublishedContentRequest` is available which represents the request that Umbraco must handle. It contains everything that will be needed to render it

All Umbraco content is looked up based on the URL in the current request using an IContentFinder. IContentFinder's you can create and implement on your own which will allow you to map any URL to an Umbraco content item.
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

Unless we are hihacking a route (when custom controllers are created to execute for different Umbraco Document Types and Templates) or have a custom implmentation (set in ApplicationStarting), everything then goes to `Umbraco.Web.Mvc.RenderMvcController` `Index()`



**Controller selection (match contoller+action to request)**
Once the published content request has been created, and MVC is the selected rendering engine, it's time to execute an MVC Controller's Action.

Any MVC Controller or Action that is attributed with `Umbraco.Web.Mvc.UmbracoAuthorizeAttribute` will authenticate the request for a backoffice user.

A SurfaceController is an MVC controller that interacts with the front-end rendering of an UmbracoPage. They can be used for rendering MVC Child Actions and for handling form data submissions. SurfaceControllers are auto-routed.



##### Execute request (MVC action+view are executed. Can query for published data)
**IPublishedContent**

**DynamicPubiishedContent (avoid as dropped in 8, but used in Zeit)**

**Umbraco Helper (use to query published media and content)**

**Members (MembershipHelper)**
MembershipHelper is a helper class for accessing member data in the form of IPublishedContent.



##### Outbound pipeline

Outbound is the process of building up a URL for a requested node.

Creates segments: “our-products”, “swibble”…

Creates paths:  “/our-products/swibble”

Creates urls: “http://site.com/our-products/swibble.aspx”

Each published content has a url segment, a.k.a. “urlName”.

#### Client Dependency

```
@RequiresCss("https://x/x/x/"); //for each file to be bundled
@RequiresJs("/js/x.js");
...
@Html.RenderCssHere() //the bundled file. rendered based on debug = true | false
@Html.RenderJsHere()
```

#### Property Editors


### Bits for later

`composition.HealthChecks().Add<FolderAndFilePermissionsCheck>();` add EPIC orders? needs 7.5+

### Links

#### the most important umbraco link ever is this:
https://our.umbraco.com/documentation/reference/common-pitfalls/

### codegardens ...
https://our.umbraco.com/videos/codegarden/


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

#### testing
https://www.jondjones.com/learn-umbraco-cms/umbraco-7-tutorials/umbraco-unit-testing/how-to-unit-test-your-umbraco-website/

http://blog.aabech.no/archive/the-basics-of-unit-testing-umbraco-just-got-simpler/

https://our.umbraco.com/forum/umbraco-8/96366-unit-testing-v8

https://skrift.io/articles/archive/unit-testing-umbraco-with-umbraco-context-mock/

https://our.umbraco.com/forum/umbraco-7/using-umbraco-7/53798-Unit-Testing

https://our.umbraco.com/documentation/Implementation/Unit-Testing/

https://garycheetham.com/2017/01/29/mocking-umbracohelper-using-dependency-injection-the-right-way/

#### others

https://www.jordanbradleyward.com/umbraco/umbraco-initial-developer-workshop/

https://our.umbraco.org/apidocs/v7/csharp/api/Umbraco.Core.html

https://www.jondjones.com/learn-umbraco-cms/umbraco-7-tutorials/

https://www.jondjones.com/learn-umbraco-cms/umbraco-7-tutorials/umbraco-deployment/how-to-configure-umbraco-to-work-in-a-load-balanced-environment/

https://farmcode.org/articles/how-to-get-umbraco-root-node-by-document-type-in-razor-using-umbraco-7-helper/

https://skrift.io/articles/archive/testing-the-performance-of-querying-umbraco/

https://our.umbraco.com/documentation/Reference/Routing/Request-Pipeline/document/TheUmbracoRequestPipeline.pdf

https://www.stephengarside.co.uk/blog/umbraco-custom-dropdown-macro-property/

https://moriyama.co.uk/about-us/news/content-versions-new-umbraco-healthcheck/

https://thesitedoctor.co.uk/blog/how-to-quickly-set-umbraco-file-and-folder-permissions-with-powershell/
