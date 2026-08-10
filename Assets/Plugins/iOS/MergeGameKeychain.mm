#import <Foundation/Foundation.h>
#import <Security/Security.h>

// 서비스 이름을 고정해 다른 앱의 Keychain 항목과 충돌하지 않게 합니다.
static NSString *const MergeGameKeychainService = @"com.happynewpuzzle.mergegame.tokens";

static NSMutableDictionary *Query(const char *key) {
    NSString *account = [NSString stringWithUTF8String:key];
    return [@{(__bridge id)kSecClass:(__bridge id)kSecClassGenericPassword,
              (__bridge id)kSecAttrService:MergeGameKeychainService,
              (__bridge id)kSecAttrAccount:account} mutableCopy];
}

extern "C" bool MergeGameKeychainSet(const char *key, const char *value) {
    NSMutableDictionary *query = Query(key);
    NSData *data = [[NSString stringWithUTF8String:value] dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary *attributes = @{(__bridge id)kSecValueData:data,
        (__bridge id)kSecAttrAccessible:(__bridge id)kSecAttrAccessibleAfterFirstUnlockThisDeviceOnly};
    OSStatus status = SecItemUpdate((__bridge CFDictionaryRef)query, (__bridge CFDictionaryRef)attributes);
    if (status == errSecItemNotFound) {
        [query addEntriesFromDictionary:attributes];
        status = SecItemAdd((__bridge CFDictionaryRef)query, NULL);
    }
    return status == errSecSuccess;
}

extern "C" char *MergeGameKeychainGet(const char *key) {
    NSMutableDictionary *query = Query(key);
    query[(__bridge id)kSecReturnData] = @YES;
    query[(__bridge id)kSecMatchLimit] = (__bridge id)kSecMatchLimitOne;
    CFTypeRef result = NULL;
    if (SecItemCopyMatching((__bridge CFDictionaryRef)query, &result) != errSecSuccess) return NULL;
    NSData *data = (__bridge_transfer NSData *)result;
    NSString *value = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    return value == nil ? NULL : strdup(value.UTF8String);
}

extern "C" void MergeGameKeychainDelete(const char *key) {
    SecItemDelete((__bridge CFDictionaryRef)Query(key));
}

extern "C" void MergeGameFreeString(char *value) { if (value != NULL) free(value); }
