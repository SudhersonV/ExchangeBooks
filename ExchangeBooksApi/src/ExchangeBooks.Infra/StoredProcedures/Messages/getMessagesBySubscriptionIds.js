function getMessagesBySubscriptionIds(subscriptionIds){
    if(!subscriptionIds, subscriptionIds.length <= 0)
        return [];
    __.chain().filter(function(doc){
        return doc.subscriptionIds.filter(function(mesageSubscriptionId){
            return subscriptionIds.filter(function(subscriptionId){
                return mesageSubscriptionId == subscriptionId;
            }).length > 0;
        }).length > 0;
    }).value();
}