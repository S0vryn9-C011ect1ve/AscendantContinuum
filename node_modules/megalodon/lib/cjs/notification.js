"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.UnknownNotificationTypeError = void 0;
var NotificationType;
(function (NotificationType) {
    NotificationType.Follow = 'follow';
    NotificationType.Favourite = 'favourite';
    NotificationType.Reblog = 'reblog';
    NotificationType.Mention = 'mention';
    NotificationType.Reaction = 'reaction';
    NotificationType.FollowRequest = 'follow_request';
    NotificationType.Status = 'status';
    NotificationType.PollVote = 'poll_vote';
    NotificationType.PollExpired = 'poll_expired';
    NotificationType.Update = 'update';
    NotificationType.Move = 'move';
    NotificationType.AdminSignup = 'admin.sign_up';
    NotificationType.AdminReport = 'admin.report';
})(NotificationType || (NotificationType = {}));
var UnknownNotificationTypeError = (function (_super) {
    __extends(UnknownNotificationTypeError, _super);
    function UnknownNotificationTypeError() {
        var _this = _super.call(this) || this;
        Object.setPrototypeOf(_this, UnknownNotificationTypeError.prototype);
        return _this;
    }
    return UnknownNotificationTypeError;
}(Error));
exports.UnknownNotificationTypeError = UnknownNotificationTypeError;
exports.default = NotificationType;
