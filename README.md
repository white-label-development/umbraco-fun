# umbraco-fun

learning umbraco...

tutorials are all a bit basic - but I am a beginner so lets see how this goes.

## snippets - mostly regarding data access, pulled from Umraco.TV

#### tv: Querying Umbraco data with Razor



`var contentModel = Model.Content.As<GameGatewayModel>(); //cast model content to our model`

`var spotModel = Model.Content.GetPropertyValue<SpotModel>("spotGame"); // "raw" umbraco IPublishedContent Id, Parent, Children etc`

where .Properties is a coll of `IPublishedProperty`

In Um there is insert dialogue that can be used to write a lot of template code for you. Check this out for syntax basics.

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

replaces Dynamic Published Content support (droped in v8). `CurrentPage.Property` etc. Probs because dynamics are a pain.
The IPublishedContent style was ` Model.Content.GetPropertyValue<T>`

Models builder stylee `@Model.BodyText` is strongly typed.

If the .cshtml inherits from `UmbracoTemplatePage<T>` we can access DynamicPublishedContent and IPublishedContent.
In reality the actual class name for T will be underlined by VS, because in the native "PureLive" mode the model is generated at runtime, so currently does not exist.

```
var x = CurrentPage.ArticlePublishDate; // DPC returns a dynamic object = meh
var y = Model.Content.GetPropertyValue<DateTime>("articlePublishDate");
vay z = Model.Content.ArticlePublishDate; // new hotness via inherts of UmbracoTemplatePage<ContentModel.NewsItem>   

```



























### Links

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

