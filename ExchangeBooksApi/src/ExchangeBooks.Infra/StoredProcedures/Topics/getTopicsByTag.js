function getTopicsByTag(searchTags){
    if(!searchTags)
        return [];
    var tagArray = searchTags.split("|");
    if(tagArray.length <= 0)
        return [];
    __.chain().filter(function(doc){
        return doc.name.split("|").filter(function(tag){
            return tagArray.filter(function(stag){
                return tag.toLowerCase() == stag.toLowerCase();
            }).length > 0;
        }).length > 0;
    }).value();
}