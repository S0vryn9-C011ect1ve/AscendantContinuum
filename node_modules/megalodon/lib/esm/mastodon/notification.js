var MastodonNotificationType;
(function (MastodonNotificationType) {
    MastodonNotificationType.Mention = 'mention';
    MastodonNotificationType.Reblog = 'reblog';
    MastodonNotificationType.Favourite = 'favourite';
    MastodonNotificationType.Follow = 'follow';
    MastodonNotificationType.Poll = 'poll';
    MastodonNotificationType.FollowRequest = 'follow_request';
    MastodonNotificationType.Status = 'status';
    MastodonNotificationType.Update = 'update';
    MastodonNotificationType.AdminSignup = 'admin.sign_up';
    MastodonNotificationType.AdminReport = 'admin.report';
})(MastodonNotificationType || (MastodonNotificationType = {}));
export default MastodonNotificationType;
