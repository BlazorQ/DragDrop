# BlazorQ DropZone
Drag and drop component for Blazor

#### Install:

Install-Package BlazorQ.DropZone

#### Usage:

1) Add BlazorDragDrop to your Startup.cs

```csharp
services.AddBlazorDragDrop();
```

2) Add BlazorQ.DragDrop to your _Imports.razor

```csharp
@using BlazorQ.DragDrop
```
3) Add drag & drop styles to your app from [dragdrop.css](BlazorQ.DragDrop/wwwroot/drop.css).

#### Create a DropZone for your items

You have to create a dropzone and assign your items to it:

```html
    <DropZone Items="MyTasks">
        <div>@context.Name</div>
    </DropZone>
```
A single DropZone makes sense, because the default value of **CanReorder** is true. So you can drag & drop items inside the only zone.

Usually you have at least two DropZones with items of the same or different type.

You can drag & drop items among DropZones with items of the same type, in other cases you need to specify **Accepts** like in the following example: 

```html
    <DropZone Items="Cats" ItemWrapperClass="@((item) => "cat")" Accepts="NewCat" OnItemDrop="SaveCatsOrder">
        <div>@context.Name</div>
        <img src="@($"api/data/photos?fileName={context.Photo}")" style="margin: 10px; width: 100px;" />
    </DropZone>

    <DropZone Items="Dogs" ItemWrapperClass="@((item) => "dog")" Accepts="NewDog">
        <div>@context.Name</div>
        <img src="@($"api/data/photos?fileName={context.Photo}")" style="margin: 10px; width: 100px;" />
    </DropZone>

    <DropZone Items="Photos" Context="fileName" CanReorder="false" Accepts="RestorePhoto">
        <img src="@($"api/data/photos?fileName={fileName}")" style="margin: 10px; width: 100px;" />
    </DropZone>
```
```cs
@code {
private Cat NewCat(object source, object target)
{
    if (source is string fileName)
        return new Cat()
        {
            Id = Guid.NewGuid(),
            Photo = fileName
        };
    else
       return null;
}

private Dog NewDog(object source, object target)
{
    if (source is string fileName)
        return new Dog()
        {
            Id = Guid.NewGuid(),
            Photo = fileName
        };
    else
       return null;
}

private string RestorePhoto(object source, object target)
{
    if (source is Cat)
        return ((Cat)source).Photo
    else if (source is Dog)
        return ((Dog)source).Photo
       return null;
}

private void SaveCatsOrder(DragDropArgs<Cat> args)
{
    int i = 0;
    foreach (var item in Cats)
       item.Order = ++i;
}

}
```
