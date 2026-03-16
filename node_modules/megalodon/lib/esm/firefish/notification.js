var FirefishNotificationType;
(function (FirefishNotificationType) {
    FirefishNotificationType.Follow = 'follow';
    FirefishNotificationType.Mention = 'mention';
    FirefishNotificationType.Reply = 'reply';
    FirefishNotificationType.Renote = 'renote';
    FirefishNotificationType.Quote = 'quote';
    FirefishNotificationType.Reaction = 'reaction';
    FirefishNotificationType.PollVote = 'pollVote';
    FirefishNotificationType.PollEnded = 'pollEnded';
    FirefishNotificationType.ReceiveFollowRequest = 'receiveFollowRequest';
    FirefishNotificationType.FollowRequestAccepted = 'followRequestAccepted';
    FirefishNotificationType.GroupInvited = 'groupInvited';
    FirefishNotificationType.App = 'app';
})(FirefishNotificationType || (FirefishNotificationType = {}));
export default FirefishNotificationType;
