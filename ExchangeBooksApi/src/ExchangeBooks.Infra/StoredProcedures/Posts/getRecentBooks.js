function getRecentBooks(count = 1){
    __.chain().sortByDescending(function(doc){
        return doc.modifiedOn;
    }, 1).pluck("books").flatten(true)
    .value()[1];
}