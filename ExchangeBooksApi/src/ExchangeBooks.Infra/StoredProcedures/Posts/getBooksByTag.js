function getBooksByTag(searchTags){
    if(!searchTags, searchTags.length <= 0)
        return [];
    __.chain().filter(function(doc){
        return doc.books.filter(function(book){
            return book.tags.filter(function(tag){
                return searchTags.filter(function(stag){
                    return stag.toLowerCase() == tag.toLowerCase();
                }).length > 0;
            }).length > 0;
        }).length > 0;
    }).value();
}