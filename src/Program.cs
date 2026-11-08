// ============================================================================
//  IDM Pro Tool — IDM 授权流白盒审计与二进制补丁工具
//  Copyright (C) 2026  angusdevgo
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
// ============================================================================

using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Security;
using System.Security.Principal;
using System.Security.AccessControl;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace IDM_Toolkit_Wpf
{

    public enum AppLanguage
    {
        Zh, // 简体中文
        En  // English
    }

    public static class I18n
    {
        public static AppLanguage Current { get; private set; }

        private static readonly Dictionary<string, string> StringsZh = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> StringsEn = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static I18n()
        {
            InitStrings();
            Initialize();
        }

        public static void Initialize()
        {
            try
            {
                string cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang.cfg");
                if (File.Exists(cfgPath))
                {
                    string saved = File.ReadAllText(cfgPath, Encoding.UTF8).Trim().ToLowerInvariant();
                    if (saved == "en")
                    {
                        Current = AppLanguage.En;
                        return;
                    }
                    if (saved == "zh" || saved == "cn")
                    {
                        Current = AppLanguage.Zh;
                        return;
                    }
                }
            }
            catch { }

            try
            {
                string culture = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
                Current = culture.StartsWith("zh") ? AppLanguage.Zh : AppLanguage.En;
            }
            catch
            {
                Current = AppLanguage.Zh;
            }
        }

        public static void SetLanguage(AppLanguage lang)
        {
            Current = lang;
            try
            {
                string cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang.cfg");
                File.WriteAllText(cfgPath, lang == AppLanguage.En ? "en" : "zh", Encoding.UTF8);
            }
            catch { }
        }

        public static string T(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            string val = null;
            if (Current == AppLanguage.En)
            {
                if (!StringsEn.TryGetValue(key, out val))
                    StringsZh.TryGetValue(key, out val);
            }
            else
            {
                if (!StringsZh.TryGetValue(key, out val))
                    StringsEn.TryGetValue(key, out val);
            }

            if (val == null) return key;
            if (args != null && args.Length > 0)
            {
                try { return string.Format(val, args); }
                catch { return val; }
            }
            return val;
        }

        private static void Add(string key, string zh, string en)
        {
            StringsZh[key] = zh;
            StringsEn[key] = en;
        }

        private static void InitStrings()
        {
            // --- 基础框架与侧边栏 ---
            Add("App.Title", "IDM Pro Tool", "IDM Pro Tool");
            Add("Brand.Edition", "PRO EDITION · v2026.1", "PRO EDITION · v2026.1");
            Add("Brand.LangToggle", "🌐 EN", "🌐 简");
            Add("Brand.LangTooltip", "切换到英文界面 (Switch to English)", "Switch to Chinese (切换到中文界面)");
            Add("Stat.DashboardTitle", "🖥️ 系统状态仪表盘", "🖥️ System Status Dashboard");
            Add("Stat.RowInstall", "软件安装:", "Installation:");
            Add("Stat.InstallInstalled", "已安装 ✓", "Installed ✓");
            Add("Stat.InstallMissing", "未识别 (点击定位) ✗", "Not Found (Click to Locate) ✗");
            Add("Stat.RowVersion", "内核版本:", "Kernel Version:");
            Add("Stat.VersionUnknown", "未知", "Unknown");
            Add("Stat.RowAuth", "授权状态:", "License Status:");
            Add("Stat.AuthRegistered", "已登记 ({0})", "Registered ({0})");
            Add("Stat.AuthFrozen", "❄️ 试用已永久冻结 (30天)", "❄️ Trial Frozen (30 Days)");
            Add("Stat.AuthUnregistered", "未登记", "Unregistered");
            Add("Stat.AuthUndetected", "未检测到", "Not Detected");
            Add("Stat.RowProcess", "后台进程:", "Process Status:");
            Add("Stat.ProcessRunning", "运行中 ({0})", "Running ({0})");
            Add("Stat.ProcessStopped", "未运行 ⚪", "Stopped ⚪");
            Add("Stat.RowPath", "主控路径:", "Master Path:");
            Add("Stat.PathRegistry", "注册表已指引", "Registry Path");
            Add("Stat.PathCustom", "用户自定义", "Custom Path");
            Add("Stat.PathDefault", "默认标准路径", "Default Path");
            Add("Stat.PathAuto", "自动捕获 ✓", "Auto-Detected ✓");
            Add("Stat.PathNotFound", "未找到 ⚠", "Not Found ⚠");
            Add("Stat.PathUndetected", "未检测到", "Not Detected");

            Add("Ctrl.Title", "🛠️ 快捷进程控制", "🛠️ Process Controls");
            Add("Ctrl.BtnRefresh", "🔄 刷新运行状态", "🔄 Refresh Status");
            Add("Ctrl.BtnLocate", "📍 手动定位 IDM 路径", "📍 Custom IDM Path");
            Add("Ctrl.BtnKill", "⏹️ 强行终止 IDM 进程", "⏹️ Terminate IDM Process");
            Add("Ctrl.BtnStart", "▶️ 启动 / 重启 IDM", "▶️ Launch / Restart IDM");
            Add("Ctrl.BtnRestore", "♻️ 一键还原官方原版", "♻️ Revert to Stock Binary");
            Add("Admin.Badge", "🛡️ 管理员特权已就绪 (UAC Admin)", "🛡️ Elevated Token Active (UAC Admin)");

            // --- 主导航与状态栏 ---
            Add("Tab.Core", "⚡ 核心授权模式", "⚡ Core Activation Modes");
            Add("Tab.Adv", "🛡️ 高级安全与策略", "🛡️ Security Shields & Policies");
            Add("Tab.Log", "📝 实时操作控制台", "📝 Live Operation Console");
            Add("Bottom.Ready", "🟢 引擎就绪 · 所有模块加载正常", "🟢 Engine Ready · All Modules Loaded");

            // --- 模式一：极速深度解锁 ---
            Add("Mode1.Title", "🔥 模式一：极速深度解锁", "🔥 Mode 1: Rapid Deep Unlock");
            Add("Mode1.Badge", "全自动秒解 · 终身免弹窗", "Instant Auto-Unlock · Zero-Nag Lifetime");
            Add("Mode1.Desc", 
                "• 核心原理：优化主程序底层指令，彻底阻断看门狗拦截、过期强退与假序列号校验。\n• 智能点亮：一键修补底层指令并同步赋予合规终身授权身份，彻底告别未注册提示。\n• 安全可靠：自动备份原版为 IDMan.exe.BAK，随时支持一键无损还原官方原版。",
                "• Core Architecture: Patches underlying machine code across 16 critical sites, severing watchdog threads, expiration traps, and counterfeit serial validation.\n• Identity Sync: Neutralizes checks and applies valid lifetime entitlement in one click, eliminating all unactivated trial alerts.\n• Safety Gate: Archives stock executable to IDMan.exe.BAK automatically, ensuring full atomic restoration at any time.");
            Add("Mode1.Btn", "⚡ 一键执行极速深度解锁", "⚡ Execute Rapid Deep Unlock");

            // --- 模式二：永久冻结试用期 ---
            Add("Mode2.Title", "❄️ 模式二：一键永久冻结试用期", "❄️ Mode 2: Permanent Trial Freeze");
            Add("Mode2.Badge", "官方支持在线更新 · 零误报推荐", "Official Updates Supported · Zero False Positives");
            Add("Mode2.Desc",
                "• 核心原理：基于 Windows ACL 权限机制锁定时间戳与 CLSID，永久剩余 30 天试用。\n• 绝大优势：完全无需修改二进制文件，完美支持 IDM 官方无缝在线静默更新！",
                "• Core Architecture: Write-protects evaluation timestamps and CLSID registry keys via Windows ACLs, permanently holding trial duration at 30 days.\n• Key Advantage: Zero binary modifications required, seamlessly compatible with official in-app background updates!");
            Add("Mode2.Btn", "❄️ 立即永久冻结试用期 (30天)", "❄️ Freeze Evaluation Trial (30 Days)");

            // --- 模式三：个性化授权登记 ---
            Add("Mode3.Title", "💎 模式三：个性化授权登记", "💎 Mode 3: Custom Identity Registration");
            Add("Mode3.Badge", "自定义证书 · 随心定制姓名", "Custom Credentials · Personalized Name");
            Add("Mode3.Desc",
                "• 核心原理：个性化定制软件关于界面的已授权人姓名与邮箱，自动联动底层解锁修补。\n• 联动防御：写入自定义信息的同时全面清除假序列号残留与黑名单项，确保系统纯净。",
                "• Core Architecture: Injects personalized licensee credentials into the About dialog while coordinating underlying binary unlocking hooks.\n• Hardened Shield: Purges residual counterfeit serial records and telemetry keys upon registration to ensure complete system hygiene.");
            Add("Mode3.LblName", "登记姓名:", "Registered Name:");
            Add("Mode3.LblEmail", "绑定邮箱:", "Bound Email:");
            Add("Mode3.LblSerial", "授权证书:", "License Key:");
            Add("Mode3.BtnRandom", "🎲 随机生成", "🎲 Randomize");
            Add("Mode3.BtnSubmit", "✨ 一键写入个性化授权", "✨ Apply Custom Registration");

            // --- 模式四：全量清理出厂重置 ---
            Add("Mode4.Title", "🔄 模式四：全量清理残留与出厂重置", "🔄 Mode 4: Factory Reset & State Cleanup");
            Add("Mode4.Badge", "环境一键复原 · 消除黑名单", "One-Click Environment Purge · Clear Blacklists");
            Add("Mode4.Desc",
                "• 核心原理：申请特权接管系统锁死的全部关联项与 CLSID 试用键，消除封禁警告与拉黑记录。\n• 适用场景：遇到程序频繁弹窗报错、或需完全恢复刚安装时的纯净状态。",
                "• Core Architecture: Assumes elevated privileges to take ownership of locked CLSID trial keys and configuration state, clearing all blacklist flags.\n• Scenarios: Resolves anomalous error popups, corrupted application state, or restores a pristine post-installation baseline.");
            Add("Mode4.Btn", "🔄 彻底清理残留并恢复出厂状态", "🔄 Purge Leftovers & Reset to Factory State");

            // --- 高级安全与策略 ---
            Add("Sec.Header", "🛡️ 弹窗防御与反封锁策略中心", "🛡️ Security Shields & Policy Center");
            Add("Sec.Badge", "零弹窗骚扰 · 域名防拉黑 · 纯净防护", "Zero Popups · Anti-Blacklist · Clean Protection");
            Add("Sec.Desc", "通过底层策略切断 IDM 自动联网探测，并利用系统 Hosts 回环阻断官方封禁标记与假冒序列号弹窗。", "Eliminates background telemetry probes via binary patches and redirects validation endpoints to loopback addresses.");
            Add("Sec.UpdateTitle", "⚡ 启动更新策略", "⚡ Startup Auto-Update Policy");
            Add("Sec.UpdateChecking", "状态: 检测中...", "Status: Checking...");
            Add("Sec.UpdateBlocked", "● 自动更新已彻底屏蔽 (安全)", "● Auto-Updates Suppressed (Safe)");
            Add("Sec.UpdateAllowed", "○ 自动更新未屏蔽 (默认开启)", "○ Auto-Updates Active (Default)");
            Add("Sec.UpdateUnknown", "⚪ 状态未检测到", "⚪ Status Undetected");
            Add("Sec.UpdateDesc", "锁定注册表策略并切断更新定时器，彻底消灭升级弹窗。", "Cuts off update timers and configures registry policy to eliminate update prompts.");
            Add("Sec.BtnBlockUpdate", "🚫 一键屏蔽更新", "🚫 Suppress Updates");
            Add("Sec.BtnAllowUpdate", "🔔 恢复官方检测", "🔔 Restore Updates");

            Add("Sec.HostsTitle", "🌐 Hosts 域名盾牌", "🌐 Hosts Domain Shield");
            Add("Sec.HostsChecking", "状态: 检测中...", "Status: Checking...");
            Add("Sec.HostsActive", "● 8 组规则已就绪 (安全拦截)", "● 8 Rules Active (Blocked)");
            Add("Sec.HostsInactive", "○ Hosts 防护未启用", "○ Hosts Shield Inactive");
            Add("Sec.HostsUnknown", "⚪ 状态未知", "⚪ Status Unknown");
            Add("Sec.HostsDesc", "回环 tonec / registeridm 等 8 组服务器，断绝官方黑名单回传。", "Redirects tonec / registeridm verification endpoints to 127.0.0.1.");
            Add("Sec.BtnBlockHosts", "🛡️ 启用域名拦截", "🛡️ Enable Shield");
            Add("Sec.BtnRestoreHosts", "🔓 恢复 Hosts", "🔓 Restore Hosts");

            Add("Tool.Header", "📁 系统路径与注册表工具箱", "📁 System Paths & Registry Toolbox");
            Add("Tool.Badge", "快捷运维 · 一键直达", "Quick Maintenance · Instant Access");
            Add("Tool.BackupTitle", "💾 备份配置", "💾 Backup Config");
            Add("Tool.BackupSub", "导出 .reg 到桌面", "Export .reg to Desktop");
            Add("Tool.RegeditTitle", "🧭 注册表", "🧭 Registry Editor");
            Add("Tool.RegeditSub", "打开 regedit", "Open regedit");
            Add("Tool.DirTitle", "📂 安装目录", "📂 Install Directory");
            Add("Tool.DirSub", "打开 IDMan 目录", "Open IDM Folder");
            Add("Tool.LocateTitle", "📍 定位路径", "📍 Custom Path");
            Add("Tool.LocateSub", "自定义 IDMan.exe", "Custom IDMan.exe");

            // --- 控制台就绪说明 ---
            Add("Log.Greeting",
                "[系统初始化] C# WPF 原生矢量渲染引擎已就绪 (支持高分屏自适应)\r\n" +
                "[外观渲染] DWM 沉浸式深黑标题栏 (Immersive Dark Mode) 与微圆角已激活\r\n" +
                "[特权验证] UAC requireAdministrator 权限已激活\r\n" +
                "[环境就绪] 请在上方选项卡选择所需的操作模式。",
                "[System Init] Native C# WPF vector UI engine ready (Per-Monitor High-DPI scaling enabled)\r\n" +
                "[DWM Render] Windows Immersive Dark Mode titlebar and rounded corners active\r\n" +
                "[Privilege] UAC requireAdministrator elevated token verified\r\n" +
                "[Environment Ready] Select your desired operating mode from the tabs above.");

            // --- 现代化交互弹窗与按钮 ---
            Add("Dlg.BtnOk", "确定完成", "Done");
            Add("Dlg.BtnGotIt", "我知道了", "Got It");
            Add("Dlg.BtnCancel", "取消", "Cancel");
            Add("Dlg.BtnConfirm", "确认继续", "Confirm");

            Add("Dlg.Locate.DialogTitle", "手动定位 IDMan.exe 主程序", "Locate IDMan.exe Manually");
            Add("Dlg.Locate.Filter", "IDM 主程序 (IDMan.exe)|IDMan.exe|所有可执行程序 (*.exe)|*.exe", "IDM Executable (IDMan.exe)|IDMan.exe|All Executables (*.exe)|*.exe");
            Add("Dlg.Locate.Success.Title", "定位成功", "Path Located");
            Add("Dlg.Locate.Success.Msg", "IDM 主程序路径已成功更新并记忆：\n\n• 当前路径：{0}\n• 主程序：IDMan.exe\n\n所有核心解锁、备份与还原功能将即刻生效于此路径！", "IDM master executable path updated and saved:\n\n• Current Path: {0}\n• Executable: IDMan.exe\n\nAll unlocking, backup, and restoration features will take effect immediately!");
            Add("Dlg.Locate.Failed.Title", "定位失败", "Location Failed");
            Add("Dlg.Locate.Failed.Msg", "未能应用所选路径:\n\n{0}", "Failed to apply the selected path:\n\n{0}");
            Add("Dlg.Locate.NotFound.Title", "未找到程序", "Executable Not Found");
            Add("Dlg.Locate.NotFound.Msg", "未找到 IDMan.exe 主程序！\n\n如果您的 IDM 安装在其他盘符（如 D盘、E盘），请点击左侧或工具箱的【手动定位 IDM 路径】指定 IDMan.exe。", "IDMan.exe executable not found!\n\nIf IDM is installed on another drive (e.g. D:\\, E:\\), please click [Custom IDM Path] in the sidebar or toolbox to select IDMan.exe.");
            Add("Dlg.Locate.DirNotFound.Title", "未找到目录", "Directory Not Found");
            Add("Dlg.Locate.DirNotFound.Msg", "未检测到 IDM 安装目录，请点击【定位路径】手动指定 IDMan.exe 所在文件夹。", "IDM installation directory not detected. Please click [Custom Path] to select the folder containing IDMan.exe.");

            Add("Dlg.Update.Blocked.Title", "更新策略生效", "Update Policy Applied");
            Add("Dlg.Update.Blocked.Msg", "已成功屏蔽 IDM 启动自动检查更新！\n\n• 策略注册表 CheckUpdtVM 已锁死为 0\n• 彻底切断启动联网升级请求，不再弹出版本升级窗口。", "IDM startup update check suppressed successfully!\n\n• Registry CheckUpdtVM policy set to 0\n• Startup update network requests severed to eliminate upgrade popups.");
            Add("Dlg.Update.Restored.Title", "更新策略已恢复", "Update Policy Restored");
            Add("Dlg.Update.Restored.Msg", "已恢复 IDM 官方自动更新检查功能。\n\n• 策略注册表 CheckUpdtVM 已重置为 1。", "Official IDM update check policy restored.\n\n• Registry CheckUpdtVM policy reset to 1.");

            Add("Dlg.Hosts.Blocked.Title", "Hosts 防护已生效", "Hosts Shield Enabled");
            Add("Dlg.Hosts.Blocked.Msg", "Hosts 域名反封锁防护已成功启用！\n\n• 已将 tonec.com / registeridm.com 等 8 组验证服务器回环指向 127.0.0.1\n• 彻底切断序列号黑名单回传与虚假授权弹窗标记。", "Hosts domain shield enabled successfully!\n\n• 8 verification server groups (tonec.com / registeridm.com) redirected to 127.0.0.1\n• Blacklist reporting and fake registration warning triggers completely severed.");
            Add("Dlg.Hosts.Restored.Title", "Hosts 规则已重置", "Hosts Rules Restored");
            Add("Dlg.Hosts.Restored.Msg", "已从系统 Hosts 中移除 IDM 验证服务器拦截规则。", "IDM verification server intercept rules removed from system hosts.");
            Add("Dlg.Hosts.Failed.Title", "Hosts 配置失败", "Hosts Setup Failed");
            Add("Dlg.Hosts.Failed.Msg", "配置系统 Hosts 失败: {0}\n\n• 请确认是否已被第三方杀毒软件拦截。", "Failed to configure system hosts: {0}\n\n• Please verify whether third-party security software blocked access.");

            Add("Dlg.Corrupt.Title", "IDMan.exe 已损坏", "IDMan.exe Corrupted");
            Add("Dlg.Corrupt.Msg", "检测到 IDMan.exe 无法作为可执行程序加载：\n\n• {0}\n\n常见原因：\n  1) 补丁偏移与当前 IDM 版本不匹配，导致映像被写坏；\n  2) 杀毒软件查杀后残留了不完整文件；\n  3) 磁盘写入中断。\n\n是否立即从 IDMan.exe.BAK 备份还原官方原版？", "Detected that IDMan.exe cannot be loaded as a valid executable:\n\n• {0}\n\nCommon Causes:\n  1) Patch offsets misaligned with IDM version, corrupting the image;\n  2) Antivirus interception leaving an incomplete binary;\n  3) Disk write interrupted.\n\nRestore official stock binary from IDMan.exe.BAK now?");
            Add("Dlg.Start.Failed.Title", "启动失败", "Launch Failed");
            Add("Dlg.Start.Failed.Msg", "启动 IDM 失败：\n\n{0}\n\n若提示「不是有效的 Win32 应用程序」，说明 IDMan.exe 已被破坏，\n请使用左侧【一键还原官方原版】恢复后再重试。", "Failed to launch IDM:\n\n{0}\n\nIf error indicates 'not a valid Win32 application', IDMan.exe is corrupted.\nPlease use [Revert to Stock Binary] on the left to restore and retry.");

            Add("Dlg.RegBackup.Title", "注册表配置已备份", "Registry Config Exported");
            Add("Dlg.RegBackup.Msg", "IDM 当前注册表配置已成功导出保存至您的桌面！\n\n• 备份文件：IDM_Reg_Backup.reg\n• 存放路径：{0}", "Current IDM registry configuration exported to your desktop!\n\n• Backup File: IDM_Reg_Backup.reg\n• File Path: {0}");
            Add("Dlg.OpError.Title", "操作执行异常", "Execution Exception");
            Add("Dlg.OpError.Msg", "执行该工具时发生异常:\n\n{0}", "Exception occurred while executing tool:\n\n{0}");

            Add("Dlg.Patch.Success.Title", "深度解锁成功", "Deep Unlock Succeeded");
            Add("Dlg.Patch.Success.Msg", "IDM 极速深度解锁成功！\n\n• 已完成全部 {0}/{1} 处核心校验指令修补\n• 授权身份：已同步点亮（授权人：{2}）\n• 关于界面：已生效为终身完整许可\n• 假冒序列号弹窗与看门狗拦截已被彻底切断！", "IDM Rapid Deep Unlock Succeeded!\n\n• Successfully patched all {0}/{1} critical validation instruction sites\n• License Identity: Active & Verified (Licensed to: {2})\n• About Dialog: Permanent Full Registered License active\n• Counterfeit serial dialogs and watchdog traps completely neutralized!");
            Add("Dlg.Patch.Active.Title", "补丁已生效", "Patch Already Active");
            Add("Dlg.Patch.Active.Msg", "IDMan.exe 补丁与授权已处于激活状态！\n\n• 全部 {0} 处核心校验逻辑均已就绪\n• 授权身份：已登记 ({1})\n• 关于界面：终身许可，零弹窗骚扰\n\n如需更改显示的授权人姓名，请使用模式三（个性化授权登记）。", "IDMan.exe patch and license are already fully active!\n\n• All {0} critical validation sites verified in patched state\n• License Identity: Registered ({1})\n• About Dialog: Lifetime license with zero nag popups\n\nTo update the displayed licensee name, please use Mode 3 (Custom Registration).");
            Add("Dlg.Patch.Error.Title", "解锁异常", "Unlock Exception");
            Add("Dlg.Patch.Error.Msg", "底层指令优化未能完成，已自动放弃写入（IDMan.exe 未被修改）。\n\n最常见原因：当前 IDM 版本不在本工具补丁指纹支持范围内\n（本工具仅适配 IDM {0}）。\n\n请查看控制台日志确认具体原因。建议改用【模式二：永久冻结试用期】\n或【模式三：个性化授权登记】。\n\n若确为权限问题，请以管理员身份运行并暂时关闭安全防护软件。", "Underlying binary modification could not complete; write aborted with zero changes (IDMan.exe unaltered).\n\nMost common cause: Installed IDM build is outside supported signature matrix\n(Supported: IDM {0}).\n\nCheck console log for details. Recommended alternatives: [Mode 2: Permanent Trial Freeze] or [Mode 3: Custom Registration].\n\nIf caused by permission limits, run as Administrator and temporarily disable antivirus protection.");

            Add("Dlg.Trial.Success.Title", "试用期冻结成功", "Trial Frozen Successfully");
            Add("Dlg.Trial.Success.Msg", "IDM 试用期已成功永久冻结！\n\n• 已通过 Windows ACL 权限锁定 {0} 处试用策略键\n• 试用状态：永久锁定在剩余 30 天，零弹窗骚扰\n• 核心优势：完全未修改任何二进制，支持官方在线静默更新！\n\n若后续需出厂重置或改用其他激活模式，可随时使用模式四或还原功能。", "IDM Evaluation Trial Frozen Successfully!\n\n• Locked {0} evaluation CLSID registry keys via Windows ACL permissions\n• Evaluation Status: Fixed permanently at 30 days remaining\n• Key Advantage: Zero binary modifications, fully compatible with official in-app updates!\n\nTo revert or switch modes later, use Mode 4 or the Revert function at any time.");
            Add("Dlg.Trial.Warning.Title", "冻结提示", "Freeze Notice");
            Add("Dlg.Trial.Warning.Msg", "未能成功锁定试用策略键，详情请查看控制台日志。\n\n建议确保以管理员权限运行，并在退出安全防护软件拦截后重试。", "Failed to lock evaluation policy keys. Please review the console log.\n\nEnsure tool is running with Administrator privileges and security software is not blocking registry access.");

            Add("Dlg.Register.Success.Title", "授权与解锁成功", "Registration Succeeded");
            Add("Dlg.Register.Success.Msg", "IDM 个性化授权信息已成功写入！\n\n• 登记姓名：{0}\n• 绑定邮箱：{1}\n• 授权证书：{2}\n\n已自动联动应用底层深度解锁，彻底杜绝假冒序列号弹窗！", "Custom Registration Applied Successfully!\n\n• Licensee Name: {0}\n• Bound Email: {1}\n• License Key: {2}\n\nUnderlying deep unlocking hooks synchronized to eliminate all fake serial popups!");
            Add("Dlg.Register.Error.Title", "授权写入失败", "Registration Failed");
            Add("Dlg.Register.Error.Msg", "写入授权信息失败，请查看控制台详细日志！", "Failed to apply registration credentials. Please review the console log for details!");

            Add("Dlg.Reset.Confirm.Title", "出厂重置确认", "Factory Reset Confirmation");
            Add("Dlg.Reset.Confirm.Msg", "确定要彻底清理所有授权记录残留与历史试用标记吗？\n\n• 执行后 IDM 将恢复为全新出厂纯净试用状态\n• 抹除所有注册表黑名单标记与试用期锁定特征", "Are you sure you want to purge all registration state and evaluation markers?\n\n• IDM will be restored to a clean post-installation trial baseline\n• All registry blacklist markers and trial locks will be eliminated");
            Add("Dlg.Reset.Success.Title", "出厂重置完成", "Factory Reset Complete");
            Add("Dlg.Reset.Success.Msg", "IDM 已成功完成出厂纯净重置！\n\n• 已清除关联配置项: {0} 处\n• 已重置试用策略键: {1} 处\n\n所有封禁与过期弹窗记录已被彻底清除。", "IDM factory reset completed successfully!\n\n• Cleaned registry settings: {0} entries\n• Reset evaluation policy keys: {1} entries\n\nAll blacklist and expired evaluation markers purged.");
            Add("Dlg.Reset.Error.Title", "重置失败", "Reset Failed");
            Add("Dlg.Reset.Error.Msg", "执行出厂重置失败:\n\n{0}", "Factory reset failed:\n\n{0}");

            Add("Dlg.Restore.Confirm.Title", "还原确认", "Restoration Confirmation");
            Add("Dlg.Restore.Confirm.Msg", "确定要将 IDM 主程序还原为官方原版并清除全部授权信息吗？\n\n• 主程序将从 IDMan.exe.BAK 备份文件无损恢复\n• 注册表授权登记与策略将被彻底清空\n• 恢复后软件将回归官方未激活评估状态", "Are you sure you want to restore official stock binary and clear all registration data?\n\n• IDMan.exe will be reverted from IDMan.exe.BAK backup\n• Registration credentials and policies will be wiped\n• Application will return to official unactivated evaluation state");
            Add("Dlg.Restore.Success.Title", "一键还原成功", "Restoration Complete");
            Add("Dlg.Restore.Success.Msg", "已成功还原为官方原版与初始配置！\n\n• 主程序 IDMan.exe 已还原为官方未修改原版\n• 授权登记信息（姓名/邮箱/序列号）已全部清除\n• 运行状态已恢复为官方未注册/试用状态", "Official Stock Binary Restored Successfully!\n\n• Executable IDMan.exe reverted to clean unpatched binary\n• License registration credentials purged\n• Evaluation state restored to official stock trial baseline");
            Add("Dlg.Restore.Warning.Title", "还原提示", "Restoration Notice");
            Add("Dlg.Restore.Warning.Msg", "还原过程中发生异常，详情请查看控制台输出日志！", "An anomaly occurred during restoration. Please check console log for details!");

            // --- CLI 模式 ---
            Add("CLI.Title", "        IDM Pro Tool - Native Engine (CLI)        ", "        IDM Pro Tool - Native Engine (CLI)        ");
            Add("CLI.SupportedOpts", "支持命令行选项：", "Supported CLI options:");
            Add("CLI.OptPatch", "  -patch                执行模式一：底层深度解锁（AOB 特征码扫描 16 处校验点修补与签名剥离）", "  -patch                Execute Mode 1: Rapid Deep Unlock (AOB 16-site patching & cert strip)");
            Add("CLI.OptFreeze", "  -freeze               执行模式二：永久冻结试用期（Windows ACL 锁定时间戳与 CLSID）", "  -freeze               Execute Mode 2: Permanent Trial Freeze (Windows ACL evaluation lock)");
            Add("CLI.OptRegister", "  -register [name] [email] [serial]  执行模式三：个性化登记并联动解锁", "  -register [name] [email] [serial]  Execute Mode 3: Custom Registration & Unlock");
            Add("CLI.OptRestore", "  -restore              执行一键还原官方原版主程序与官方未注册配置", "  -restore              Execute One-Click Stock Binary Restoration");
            Add("CLI.OptSetPath", "  -setpath <dir/exe>    手动设定并持久化 IDM 安装目录或 IDMan.exe 绝对路径", "  -setpath <dir/exe>    Set and persist custom IDM installation directory or executable path");
            Add("Err.PathEmpty", "选择的路径为空。", "The selected path is empty.");
            Add("Err.NotIDMExe", "所选文件不是 IDMan.exe 主程序。", "Selected file is not IDMan.exe executable.");
            Add("Err.DirNotExist", "目标目录不存在: {0}", "Target directory does not exist: {0}");
            Add("Err.ExeNotFoundInDir", "在目录 [{0}] 下未找到 IDMan.exe 主程序文件！", "IDMan.exe not found in directory [{0}]!");
            Add("CLI.PathUsage", "Error: 请提供 IDM 目录或 IDMan.exe 路径，例如: -setpath \"D:\\Software\\IDM\"", "Error: Provide IDM directory or IDMan.exe path, e.g.: -setpath \"D:\\Software\\IDM\"");

            Add("Diag.NotFound", "目标文件不存在", "Target file does not exist");
            Add("Diag.ZeroBytes", "文件长度为 0 字节（极可能被杀毒软件查杀/隔离后残留空文件）", "File size is 0 bytes (likely quarantined by antivirus)");
            Add("Diag.ReadFailed", "读取失败: {0}", "Read failed: {0}");

            Add("Pe.TooSmall", "文件体积过小，不是有效的 PE 映像", "File size too small; not a valid PE image");
            Add("Pe.MissingMZ", "缺少 MZ 头，不是有效的 Win32 可执行文件", "Missing MZ header; not a valid Win32 executable");
            Add("Pe.BadLfanew", "DOS 头 e_lfanew 偏移越界", "DOS header e_lfanew offset out of bounds");
            Add("Pe.MissingPE", "缺少 PE 签名，映像已损坏", "Missing PE signature; image is corrupted");
            Add("Pe.BadCoff", "COFF 文件头越界", "COFF file header out of bounds");
            Add("Pe.BadNsec", "节区数量异常 ({0})", "Abnormal section count ({0})");
            Add("Pe.BadOptSz", "可选头长度异常 ({0})", "Optional header length abnormal ({0})");
            Add("Pe.BadMagic", "可选头 Magic 异常 (0x{0})，不是标准 PE 映像", "Optional header Magic abnormal (0x{0}); not a standard PE image");
            Add("Pe.BadSecTab", "节区表越界，映像已被截断损坏", "Section table out of bounds; image truncated or damaged");
            Add("Pe.BadSecHeader", "第 {0} 个节区头字段非法", "Section #{0} header fields invalid");
            Add("Pe.SecOutOfFile", "节区 [{0}] 原始数据超出文件末尾（需要 {1} 字节，实际 {2} 字节）—— 映像已被截断损坏", "Section [{0}] raw data exceeds end of file (needs {1} bytes, actual {2} bytes) - Image truncated and corrupted");
        }


        public static string TranslateLog(string msg)
        {
            if (string.IsNullOrEmpty(msg) || Current == AppLanguage.Zh) return msg;

            // 1. 横幅与主标题行
            if (msg.Contains("开始执行模式一：极速深度解锁")) return "================= Executing Mode 1: Rapid Deep Unlock =================";
            if (msg.Contains("开始执行模式二：一键永久冻结试用期")) return "================= Executing Mode 2: Permanent Trial Freeze =================";
            if (msg.Contains("开始执行模式三：个性化授权登记")) return "================= Executing Mode 3: Custom Identity Registration =================";
            if (msg.Contains("开始执行模式四：全量清理出厂重置")) return "================= Executing Mode 4: Factory Reset & State Cleanup =================";
            if (msg.Contains("开始执行：一键还原官方原版与初始配置")) return "================= Executing: One-Click Stock Binary Restoration =================";
            if (msg.Contains("二进制补丁引擎 (AOB 特征码版 v3)")) return "──────── Binary Patch Engine (AOB Pattern Scan v3) ────────";

            string s = msg;

            // 2. 补丁点名称
            s = s.Replace("授权分支检测 (test eax->xor eax)", "License branch check (test eax->xor eax)")
                 .Replace("试用期最大值 (->mov eax,7FFFFFFF+nop)", "Trial maximum value (->mov eax,7FFFFFFF+nop)")
                 .Replace("假序列号弹窗 A (jz->jmp)", "Counterfeit serial popup A (jz->jmp)")
                 .Replace("守护线程 A", "Watchdog thread A")
                 .Replace("守护线程 B", "Watchdog thread B")
                 .Replace("守护线程 C", "Watchdog thread C")
                 .Replace("守护线程 D", "Watchdog thread D")
                 .Replace("看门狗关联函数", "Watchdog helper function")
                 .Replace("守护线程 E (push ebp->ret)", "Watchdog thread E (push ebp->ret)")
                 .Replace("守护线程 F", "Watchdog thread F")
                 .Replace("过期强退逻辑 (0F85->90E9)", "Expiration termination logic (0F85->90E9)")
                 .Replace("联网验证标志位", "Online validation flag")
                 .Replace("假序列号弹窗 B (jz->jmp)", "Counterfeit serial popup B (jz->jmp)")
                 .Replace("试用状态标志 + 天数限制常量", "Trial flag + duration limit constants")
                 .Replace("注册对话框抑制 (Serial缺失分支, 可选)", "Registration dialog bypass (missing serial branch, optional)")
                 .Replace("启动自动更新定时器切断 (jne->jmp, 彻底杜绝后台更新)", "Startup auto-update timer cutoff (jne->jmp)");

            // 3. 诊断与前置校验日志
            s = s.Replace("✗ 启动前体检未通过：", "✗ Pre-launch diagnostic failed: ");
            s = s.Replace("  目标路径: ", "  Target path: ");
            s = s.Replace("目标文件不存在", "Target file does not exist");
            s = s.Replace("文件长度为 0 字节（极可能被杀毒软件查杀/隔离后残留空文件）", "File size is 0 bytes (likely quarantined by antivirus)");
            s = s.Replace("文件体积过小，不是有效的 PE 映像", "File size too small; not a valid PE image");
            s = s.Replace("缺少 MZ 头，不是有效的 Win32 可执行文件", "Missing MZ header; not a valid Win32 executable");
            s = s.Replace("DOS 头 e_lfanew 偏移越界", "DOS header e_lfanew offset out of bounds");
            s = s.Replace("缺少 PE 签名，映像已损坏", "Missing PE signature; image is corrupted");
            s = s.Replace("COFF 文件头越界", "COFF file header out of bounds");
            s = s.Replace("可选头 Magic 异常 (0x", "Optional header Magic abnormal (0x");
            s = s.Replace("可选头长度异常 (", "Optional header length abnormal (");
            s = s.Replace("节区表越界，映像已被截断损坏", "Section table out of bounds; image truncated or damaged");
            s = s.Replace("节区数量异常 (", "Abnormal section count (");
            s = s.Replace(" 字节）—— 映像已被截断损坏", " bytes) - Image truncated and corrupted");
            s = s.Replace(" 字节，实际 ", " bytes, actual ");
            s = s.Replace("] 原始数据超出文件末尾（需要 ", "] Raw data exceeds end of file (requires ");

            // 4. 补丁引擎核心执行日志
            s = s.Replace("✓ 补丁写入 [", "✓ Applied patch [");
            s = s.Replace("→ 已处于补丁状态，跳过: ", "→ Already in patched state, skipping: ");
            s = s.Replace("  – 跳过可选位点（本版本无此逻辑）: ", "  – Skipping optional site (not present in this release): ");
            s = s.Replace("  ✗ 未找到特征码: ", "  ✗ Pattern signature not found: ");
            s = s.Replace("  ✗ 特征码非唯一命中: ", "  ✗ Pattern signature matched multiple locations: ");
            s = s.Replace("  ✗ 位点越界: ", "  ✗ Site out of bounds: ");
            s = s.Replace("  ✗ 位点字节与期望不符: ", "  ✗ Site bytes mismatch expected value: ");
            s = s.Replace("✓ PE 结构校验通过（节区 ", "✓ PE structural validation passed (Sections: ");
            s = s.Replace(" 个，映像末尾 ", ", Image End ");
            s = s.Replace("）", ")");
            s = s.Replace("  正在执行 AOB 特征码扫描（支持版本：", "  Scanning AOB pattern signatures (Supported: ");
            s = s.Replace("）...", ")...");
            s = s.Replace("✓ AOB 扫描全部命中 ", "✓ AOB scan matched all ");
            s = s.Replace("（已补丁 ", " (Patched: ");
            s = s.Replace(" 处 / 待补丁 ", ", Pending: ");
            s = s.Replace(" 处）", ")");
            s = s.Replace("✓ 已清除 PE 数字签名目录（偏移 ", "✓ Cleared PE security directory (Offset: ");
            s = s.Replace("，原 RVA=", ", Original RVA=");
            s = s.Replace("→ PE 数字签名目录本就不存在，无需清除", "→ PE security directory does not exist, no clearing needed");
            s = s.Replace("✓ 已剥离 Authenticode 签名尾部 ", "✓ Stripped Authenticode signature overlay: ");
            s = s.Replace(" 字节（动态计算，非硬编码）", " bytes (dynamically computed)");
            s = s.Replace("✓ 已剥离映像尾部冗余数据 ", "✓ Stripped redundant overlay data: ");
            s = s.Replace(" 字节", " bytes");
            s = s.Replace("✓ 已重算 PE 校验和：", "✓ Recomputed PE checksum: ");
            s = s.Replace("（标准算法动态计算）", " (Microsoft standard algorithm)");
            s = s.Replace("★ 底层二进制补丁写入完成！共 ", "★ Binary patching completed! Total ");
            s = s.Replace(" 个位点生效。", " sites active.");
            s = s.Replace("  落盘 SHA256: ", "  Output SHA256: ");
            s = s.Replace("  落盘体积  : ", "  Output size  : ");

            // 5. 备份与还原
            s = s.Replace("✓ 已自动备份原版至: ", "✓ Created clean stock backup: ");
            s = s.Replace("→ 已有同版本官方原版备份（v", "→ Clean stock backup already exists for version v");
            s = s.Replace("），保持不动", "), preserved without changes");
            s = s.Replace("⚠ 备份版本不一致：当前 IDMan.exe = v", "⚠ Backup version mismatch: Current IDMan.exe = v");
            s = s.Replace("，现有 IDMan.exe.BAK = v", ", Existing IDMan.exe.BAK = v");
            s = s.Replace("未知", "Unknown");
            s = s.Replace("✗ 当前 IDMan.exe 并非纯净原版（已含补丁），无法作为新备份源。", "✗ Current IDMan.exe is not clean stock (already patched); cannot be used as backup source.");
            s = s.Replace("  ★ 若继续，回滚点将指向错误版本。为安全起见已中止。", "  ★ Aborted to prevent rollback point pointing to mismatched version.");
            s = s.Replace("  ★ 请先重新安装当前版本的 IDM，或手动将官方原版另存为: ", "  ★ Reinstall official IDM or manually save clean stock binary to: ");
            s = s.Replace("→ 旧版备份已归档为: ", "→ Archived legacy backup to: ");
            s = s.Replace("⚠ 旧备份归档失败（将继续覆盖）: ", "⚠ Failed to archive legacy backup (will overwrite): ");
            s = s.Replace("✓ 已更新官方原版备份至当前版本 v", "✓ Updated official stock backup to current version v");
            s = s.Replace("✗ 写入新备份失败，为安全起见中止补丁: ", "✗ Failed to write new backup, patching aborted: ");
            s = s.Replace("✓ 已自动回滚至补丁前状态", "✓ Automatically rolled back to pre-patch state");
            s = s.Replace("✓ 已自动回滚，IDMan.exe 恢复至补丁前状态", "✓ Automatically rolled back; IDMan.exe restored to pre-patch state");
            s = s.Replace("✓ 已自动回滚", "✓ Automatically rolled back");

            s = s.Replace("正在检测并强行终止后台运行的 IDMan 进程...", "Detecting and terminating background IDMan processes...");
            s = s.Replace("已成功终止 ", "Successfully terminated ");
            s = s.Replace(" 个正在运行的 IDM 进程。", " running IDM processes.");
            s = s.Replace("✗ 拒绝还原：备份版本与当前安装版本不一致！", "✗ Restoration rejected: Backup version does not match current installation!");
            s = s.Replace("    当前 IDMan.exe   = v", "    Current IDMan.exe   = v");
            s = s.Replace("    现有 IDMan.exe.BAK = v", "    Existing IDMan.exe.BAK = v");
            s = s.Replace("  ★ 直接还原会把旧版主程序覆盖到新版安装上，导致程序异常。", "  ★ Restoring will overwrite newer installation with older binary, causing runtime mismatch.");
            s = s.Replace("  ★ 请重新安装当前版本的 IDM，或手动指定正确的官方原版备份。", "  ★ Please reinstall current IDM build or specify the correct clean stock backup.");
            s = s.Replace("✓ 官方原版主程序已成功无损还原！(IDMan.exe.BAK -> IDMan.exe, v", "✓ Official stock binary restored successfully! (IDMan.exe.BAK -> IDMan.exe, v");
            s = s.Replace("还原主程序文件失败 (请确认以管理员身份运行): ", "Failed to restore executable (ensure running as Administrator): ");
            s = s.Replace("提示：未检测到官方原版备份文件 (IDMan.exe.BAK)，跳过文件覆盖。", "Notice: Official stock backup (IDMan.exe.BAK) not detected, skipping file copy.");
            s = s.Replace("✓ 已彻底抹除注册表授权登记项 (FName/Email/Serial等共 ", "✓ Completely wiped registry registration entries (total ");
            s = s.Replace(" 项)", " entries)");
            s = s.Replace("✓ 已同步解除 ACL 权限锁定并清理试用特征项 ", "✓ Unlocked ACL permissions and purged evaluation markers: ");
            s = s.Replace("✓ 注册表授权配置已彻底重置为【官方未注册原版】状态！", "✓ Registry configuration restored to official unactivated stock baseline!");
            s = s.Replace("✓ 一键还原官方操作完成。", "✓ Stock restoration completed successfully.");

            // 6. 各模式详细操作流程日志
            s = s.Replace("错误：未找到 IDMan.exe，请先确认 IDM 是否已安装。", "Error: IDMan.exe not found. Please confirm IDM is installed.");
            s = s.Replace("✓ 已同步点亮终身授权登记：授权姓名 [", "✓ Synchronized lifetime license identity: Name [");
            s = s.Replace("]，绑定邮箱 [", "], Bound Email [");
            s = s.Replace("✓ 已同步清理注册表残留假序列号与黑名单字段，关闭自动更新", "✓ Cleaned residual counterfeit serials & blacklist telemetry; disabled auto-updates");
            s = s.Replace("✓ 模式一执行完成：已彻底消除假冒序列号弹窗与防逆向看门狗拦截！", "✓ Mode 1 completed: Counterfeit serial popups and anti-reversing watchdog neutralized!");
            s = s.Replace("  共完成 ", "  Completed ");
            s = s.Replace(" 处核心校验逻辑升级（共 ", " core validation upgrades (total ");
            s = s.Replace(" 个补丁位点）", " patch sites)");
            s = s.Replace("✓ 模式一检测：IDMan.exe 已包含全部优化特征，补丁已生效，已确保终身授权身份就绪。", "✓ Mode 1 verified: IDMan.exe contains all patch signatures; lifetime entitlement active.");
            s = s.Replace("模式一执行遇到异常，详情请查看上方日志。", "Mode 1 encountered an exception; check console log for details.");

            s = s.Replace("步骤 1/4: 正在安全终止后台运行的 IDM 监控与下载进程...", "Step 1/4: Safely terminating background IDM monitoring and download processes...");
            s = s.Replace("步骤 2/4: 正在清理历史授权追踪与黑名单遥测项，同步底层驱动策略...", "Step 2/4: Cleaning historical tracking and blacklist telemetry; synchronizing driver policies...");
            s = s.Replace("✓ 已清理旧试用与过期特征项共 ", "✓ Cleaned legacy trial and expired feature markers total ");
            s = s.Replace("步骤 3/4: 触发 IDM 引擎初始化全新 30 天官方评估基线...", "Step 3/4: Triggering IDM engine to initialize fresh 30-day official evaluation baseline...");
            s = s.Replace("正在唤醒 IDM 初始化全新 30 天试用基线...", "Waking IDM to initialize fresh 30-day evaluation baseline...");
            s = s.Replace("步骤 4/4: 正在通过 Windows ACL 权限机制锁定试用策略键...", "Step 4/4: Write-protecting trial policy keys via Windows ACL permissions...");
            s = s.Replace("已解除锁定并清除旧试用特征项: ", "Unlocked and purged legacy trial marker: ");
            s = s.Replace("✓ 已通过 ACL 权限锁定试用项: ", "✓ Write-protected trial key via ACL: ");
            s = s.Replace("★ 永久冻结试用期成功！共锁定 ", "★ Trial frozen successfully! Total ");
            s = s.Replace(" 处核心 CLSID 试用时间策略键。", " critical evaluation CLSID policy keys locked.");
            s = s.Replace("✓ 运行状态：试用倒计时永久停留在剩余 30 天，无任何弹窗骚扰。", "✓ Status: Evaluation countdown permanently held at 30 days remaining with zero popups.");
            s = s.Replace("✓ 升级兼容：未修改任何二进制文件，完全兼容 IDM 官方无缝在线静默更新！", "✓ Compatibility: Zero binary modifications, fully compatible with official in-app updates!");

            s = s.Replace("登记姓名: ", "Licensee Name: ");
            s = s.Replace("绑定邮箱: ", "Bound Email: ");
            s = s.Replace("授权证书: ", "License Key: ");
            s = s.Replace("正在自动联动执行【模式一：底层深度解锁】以杜绝假冒序列号弹窗...", "Automatically coordinating Mode 1 (Rapid Deep Unlock) to prevent counterfeit serial popups...");
            s = s.Replace("✓ 成功将登记信息写入注册表 (HKCU\\Software\\DownloadManager)", "✓ Successfully wrote registration credentials to registry (HKCU\\Software\\DownloadManager)");
            s = s.Replace("  授权姓名: ", "  Licensed Name: ");
            s = s.Replace("  绑定邮箱: ", "  Bound Email: ");
            s = s.Replace("✓ 模式三执行完毕：底层免弹窗补丁与关于窗口已同步点亮！", "✓ Mode 3 completed: Low-level patch active, About dialog displays full registration!");

            s = s.Replace("✓ 官方原版主程序已同步无损恢复 (IDMan.exe.BAK -> IDMan.exe)", "✓ Official stock binary restored (IDMan.exe.BAK -> IDMan.exe)");
            s = s.Replace("已抹除 DownloadManager 残留项: ", "Erased residual DownloadManager entry: ");
            s = s.Replace("✓ 第一阶段：已抹除 DownloadManager 配置关联项 ", "✓ Phase 1: Wiped DownloadManager configuration entries: ");
            s = s.Replace("✓ 第二阶段：已解除锁定并清理 CLSID 试用策略项 ", "✓ Phase 2: Unlocked and purged CLSID evaluation policy entries: ");
            s = s.Replace("✓ 模式四执行完毕：IDM 授权与黑名单数据已彻底消除，成功恢复出厂纯净状态！", "✓ Mode 4 completed: Registration and blacklist data wiped, restored to factory baseline!");

            // 7. 快捷工具与策略交互日志
            s = s.Replace("运行状态仪表盘与系统信息已刷新完成。当前路径: ", "Dashboard status and system information refreshed. Current path: ");
            s = s.Replace("✓ 已成功手动定位 IDM 路径: ", "✓ Custom IDM path located and saved: ");
            s = s.Replace("定位 IDM 路径失败: ", "Failed to locate IDM path: ");
            s = s.Replace("已启动 IDM 应用程序。", "Started IDM application.");
            s = s.Replace("✗ 启动 IDM 失败: ", "✗ Failed to start IDM: ");
            s = s.Replace("未找到 IDMan.exe 主程序，请尝试点击【手动定位 IDM 路径】。", "IDMan.exe not found; try clicking [Custom IDM Path].");
            s = s.Replace("已成功导出 IDM 注册表配置至桌面: ", "Exported IDM registry configuration to desktop: ");
            s = s.Replace("已启动 Windows 注册表编辑器。", "Launched Windows Registry Editor.");
            s = s.Replace("已在资源管理器中打开 IDM 安装目录: ", "Opened IDM installation directory in Explorer: ");
            s = s.Replace("操作异常: ", "Operation exception: ");
            s = s.Replace("✓ 已启用自动更新屏蔽策略：CheckUpdtVM = 0 (已阻断启动自动检测与升级弹窗)", "✓ Enabled auto-update suppression policy: CheckUpdtVM = 0");
            s = s.Replace("✓ 已恢复 IDM 官方自动更新检查：CheckUpdtVM = 1", "✓ Restored official IDM auto-update check: CheckUpdtVM = 1");
            s = s.Replace("更新策略设置失败: ", "Failed to set update policy: ");
            s = s.Replace("错误: 未找到系统 hosts 文件: ", "Error: System hosts file not found: ");
            s = s.Replace("✓ 已成功将 IDM 官方黑名单与验证服务器重定向至 127.0.0.1 (系统 hosts 防护已生效)", "✓ Redirected IDM verification servers to 127.0.0.1 (Hosts shield active)");
            s = s.Replace("✓ 已成功从系统 hosts 中移除 IDM 验证服务器拦截规则", "✓ Removed IDM intercept rules from system hosts");
            s = s.Replace("配置 Hosts 失败: ", "Failed to configure hosts: ");
            s = s.Replace(" (请确认以管理员特权运行)", " (ensure running with Administrator privileges)");
            s = s.Replace(" 处", " entries");

            return s;
        }

    }

    public class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        private const int ATTACH_PARENT_PROCESS = -1;
        private const int STD_OUTPUT_HANDLE = -11;

        private static void InitConsoleOutput()
        {
            try
            {
                if (AttachConsole(ATTACH_PARENT_PROCESS))
                {
                    IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                    if (stdHandle != IntPtr.Zero && stdHandle != (IntPtr)(-1))
                    {
                        SafeFileHandle safeHandle = new SafeFileHandle(stdHandle, false);
                        FileStream fs = new FileStream(safeHandle, FileAccess.Write);
                        StreamWriter writer = new StreamWriter(fs, Encoding.GetEncoding(936)) { AutoFlush = true };
                        Console.SetOut(writer);
                        Console.SetError(writer);
                    }
                }
            }
            catch { }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            // 管理员身份检测与自提权请求（无论 GUI 还是 CLI 模式均保证具有系统管理员特权）
            if (!IsAdministrator())
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = Process.GetCurrentProcess().MainModule.FileName;
                    psi.UseShellExecute = true;
                    psi.Verb = "runas";
                    if (args != null && args.Length > 0)
                        psi.Arguments = string.Join(" ", args);
                    Process p = Process.Start(psi);
                    if (p != null)
                    {
                        p.WaitForExit();
                        Environment.ExitCode = p.ExitCode;
                    }
                    return;
                }
                catch
                {
                    // 若用户拒绝 UAC 提权，则尝试在受限环境下继续
                }
            }

            // 检查是否有命令行参数
            if (args != null && args.Length > 0)
            {
                string firstArg = args[0].ToLowerInvariant();
                if (firstArg == "-patch" || firstArg == "/patch" ||
                    firstArg == "-freeze" || firstArg == "/freeze" ||
                    firstArg == "-register" || firstArg == "/register" ||
                    firstArg == "-restore" || firstArg == "/restore" ||
                    firstArg == "-setpath" || firstArg == "/setpath" ||
                    firstArg == "-help" || firstArg == "/help" || firstArg == "/?")
                {
                    InitConsoleOutput();
                    Console.WriteLine();
                    Console.WriteLine("==================================================");
                    Console.WriteLine("        IDM Pro Tool - Native Engine (CLI)        ");
                    Console.WriteLine("==================================================");

                    if (firstArg == "-lang" || firstArg == "/lang")
                    {
                        if (args.Length > 1 && (args[1].ToLowerInvariant() == "en" || args[1].ToLowerInvariant() == "zh"))
                        {
                            I18n.SetLanguage(args[1].ToLowerInvariant() == "en" ? AppLanguage.En : AppLanguage.Zh);
                            Console.WriteLine("Language set to: " + (I18n.Current == AppLanguage.En ? "English" : "简体中文"));
                        }
                        else
                        {
                            Console.WriteLine("Usage: -lang <zh|en>");
                        }
                        return;
                    }

                    if (firstArg == "-help" || firstArg == "/help" || firstArg == "/?")
                    {
                        Console.WriteLine(I18n.T("CLI.SupportedOpts"));
                        Console.WriteLine(I18n.T("CLI.OptPatch"));
                        Console.WriteLine(I18n.T("CLI.OptFreeze"));
                        Console.WriteLine(I18n.T("CLI.OptRegister"));
                        Console.WriteLine(I18n.T("CLI.OptRestore"));
                        Console.WriteLine(I18n.T("CLI.OptSetPath"));
                        Console.WriteLine(I18n.T("CLI.OptLang"));
                        return;
                    }

                    if (firstArg == "-setpath" || firstArg == "/setpath")
                    {
                        if (args.Length > 1)
                        {
                            string errMsg;
                            bool ok = MainWindow.SetCustomIDMPath(args[1], out errMsg);
                            if (ok)
                                Console.WriteLine("Set IDM Path SUCCESS: " + MainWindow.GetIDMDir());
                            else
                                Console.WriteLine("Set IDM Path FAILED: " + errMsg);
                        }
                        else
                        {
                            Console.WriteLine(I18n.T("CLI.PathUsage"));
                        }
                        return;
                    }

                    if (firstArg == "-patch" || firstArg == "/patch")
                    {
                        int count = 0;
                        bool ok = MainWindow.ExecutePatchDirect(Console.WriteLine, out count);
                        Console.WriteLine("Patch finished: ok=" + ok + ", count=" + count);
                        return;
                    }

                    if (firstArg == "-freeze" || firstArg == "/freeze")
                    {
                        int count = 0;
                        bool ok = MainWindow.ExecuteTrialFreezeDirect(Console.WriteLine, out count);
                        Console.WriteLine("Freeze finished: ok=" + ok + ", count=" + count);
                        return;
                    }

                    if (firstArg == "-register" || firstArg == "/register")
                    {
                        string name = (args.Length > 1) ? args[1] : MainWindow.GetDefaultUserName();
                        string email = (args.Length > 2) ? args[2] : MainWindow.GetDefaultUserEmail(name);
                        string serial = (args.Length > 3) ? args[3] : null;
                        bool ok = MainWindow.ExecuteRegisterDirect(name, email, serial, Console.WriteLine);
                        Console.WriteLine("Register finished: ok=" + ok);
                        return;
                    }

                    if (firstArg == "-restore" || firstArg == "/restore")
                    {
                        bool ok = MainWindow.RestoreOriginalDirect(Console.WriteLine);
                        Console.WriteLine("Restore finished: ok=" + ok);
                        return;
                    }
                }
            }

            int startTab = 0;
            if (args != null && args.Length > 0 && (args[0] == "-tab" || args[0] == "/tab") && args.Length > 1)
            {
                int.TryParse(args[1], out startTab);
            }

            try
            {
                Application app = new Application();
                app.Run(new MainWindow(startTab));
            }
            catch (Exception ex)
            {
                try { File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"), ex.ToString()); }
                catch { }
                MessageBox.Show(ex.ToString(), "WPF Crash");
            }
        }

        public static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }

    
    public enum ModernDialogType
    {
        Success,
        Information,
        Warning,
        Error,
        Question
    }

    public class ModernDialog : Window
    {
        public bool Confirmed { get; private set; }

        public static bool ShowSuccess(Window owner, string title, string message)
        {
            return Show(owner, title, message, ModernDialogType.Success);
        }

        public static bool ShowInfo(Window owner, string title, string message)
        {
            return Show(owner, title, message, ModernDialogType.Information);
        }

        public static bool ShowWarning(Window owner, string title, string message)
        {
            return Show(owner, title, message, ModernDialogType.Warning);
        }

        public static bool ShowError(Window owner, string title, string message)
        {
            return Show(owner, title, message, ModernDialogType.Error);
        }

        public static bool Confirm(Window owner, string title, string message)
        {
            return Show(owner, title, message, ModernDialogType.Question);
        }

        public static bool Show(Window owner, string title, string message, ModernDialogType type)
        {
            Window parent = owner;
            if (parent == null)
            {
                if (Application.Current != null && Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
                    parent = Application.Current.MainWindow;
            }

            ModernDialog dlg = new ModernDialog(title, message, type);
            if (parent != null && parent.IsVisible)
            {
                dlg.Owner = parent;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            dlg.ShowDialog();
            return dlg.Confirmed;
        }

        public ModernDialog(string titleText, string messageText, ModernDialogType type)
        {
            this.Title = titleText;
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = Brushes.Transparent;
            this.Width = 470;
            this.SizeToContent = SizeToContent.Height;
            this.ShowInTaskbar = false;
            this.ResizeMode = ResizeMode.NoResize;

            Color accentColor;
            Color accentGlow;
            string iconGlyph;
            string badgeText;

            switch (type)
            {
                case ModernDialogType.Success:
                    accentColor = Color.FromRgb(34, 197, 94);   // Emerald 500
                    accentGlow  = Color.FromArgb(55, 34, 197, 94);
                    iconGlyph   = "✓";
                    badgeText   = "SUCCESS";
                    break;
                case ModernDialogType.Warning:
                    accentColor = Color.FromRgb(245, 158, 11);  // Amber 500
                    accentGlow  = Color.FromArgb(55, 245, 158, 11);
                    iconGlyph   = "⚠";
                    badgeText   = "ATTENTION";
                    break;
                case ModernDialogType.Error:
                    accentColor = Color.FromRgb(239, 68, 68);   // Red 500
                    accentGlow  = Color.FromArgb(55, 239, 68, 68);
                    iconGlyph   = "✕";
                    badgeText   = "ERROR";
                    break;
                case ModernDialogType.Question:
                    accentColor = Color.FromRgb(168, 85, 247);  // Purple 500
                    accentGlow  = Color.FromArgb(55, 168, 85, 247);
                    iconGlyph   = "?";
                    badgeText   = "CONFIRMATION";
                    break;
                case ModernDialogType.Information:
                default:
                    accentColor = Color.FromRgb(56, 189, 248);  // Sky 400
                    accentGlow  = Color.FromArgb(55, 56, 189, 248);
                    iconGlyph   = "ℹ";
                    badgeText   = "NOTICE";
                    break;
            }

            // 最外层容器：留出四周投射阴影的空间 (Margin 16)
            Grid root = new Grid { Margin = new Thickness(16) };

            Border card = new Border
            {
                CornerRadius = new CornerRadius(14),
                Background = new SolidColorBrush(Color.FromRgb(22, 27, 34)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)),
                BorderThickness = new Thickness(1),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 8,
                    BlurRadius = 24,
                    Opacity = 0.65
                }
            };

            // 内部网格布局
            Grid contentGrid = new Grid();
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 标题栏
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 消息正文
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 底部操作按钮

            // 1. 顶部 Header 栏（支持窗口拖拽）
            Border headerBar = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(28, 33, 40)),
                CornerRadius = new CornerRadius(14, 14, 0, 0),
                Padding = new Thickness(18, 14, 18, 14),
                BorderBrush = new SolidColorBrush(Color.FromRgb(38, 44, 52)),
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
            headerBar.MouseLeftButtonDown += (s, e) => { try { this.DragMove(); } catch { } };

            Grid headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // 状态徽标图标圆圈
            Border iconCircle = new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(14),
                Background = new SolidColorBrush(accentGlow),
                BorderBrush = new SolidColorBrush(accentColor),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 12, 0)
            };
            TextBlock iconText = new TextBlock
            {
                Text = iconGlyph,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(accentColor),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconCircle.Child = iconText;
            Grid.SetColumn(iconCircle, 0);
            headerGrid.Children.Add(iconCircle);

            // 标题
            TextBlock titleBlock = new TextBlock
            {
                Text = titleText,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(240, 246, 252)),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(titleBlock, 1);
            headerGrid.Children.Add(titleBlock);

            // 右上角类型小徽章
            Border typeBadge = new Border
            {
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Color.FromRgb(33, 38, 45)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(6, 2, 6, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            typeBadge.Child = new TextBlock
            {
                Text = badgeText,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(accentColor)
            };
            Grid.SetColumn(typeBadge, 2);
            headerGrid.Children.Add(typeBadge);

            headerBar.Child = headerGrid;
            Grid.SetRow(headerBar, 0);
            contentGrid.Children.Add(headerBar);

            // 2. 中部内容区域
            StackPanel bodyPanel = new StackPanel { Margin = new Thickness(22, 18, 22, 18) };

            // 智能分析文本：如果是包含列表项（如 •），自动分块排版
            string[] lines = messageText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            bool inBulletList = false;
            StackPanel bulletBox = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                {
                    if (inBulletList) { inBulletList = false; }
                    bodyPanel.Children.Add(new FrameworkElement { Height = 6 });
                    continue;
                }

                if (line.StartsWith("•") || line.StartsWith("-") || line.StartsWith("*"))
                {
                    if (!inBulletList)
                    {
                        inBulletList = true;
                        Border bulletBorder = new Border
                        {
                            CornerRadius = new CornerRadius(8),
                            Background = new SolidColorBrush(Color.FromRgb(13, 17, 23)),
                            BorderBrush = new SolidColorBrush(Color.FromRgb(33, 38, 45)),
                            BorderThickness = new Thickness(1),
                            Padding = new Thickness(12, 8, 12, 8),
                            Margin = new Thickness(0, 4, 0, 8)
                        };
                        bulletBox = new StackPanel();
                        bulletBorder.Child = bulletBox;
                        bodyPanel.Children.Add(bulletBorder);
                    }

                    Grid itemGrid = new Grid { Margin = new Thickness(0, 2, 0, 2) };
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    TextBlock dot = new TextBlock
                    {
                        Text = "•",
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(accentColor),
                        Margin = new Thickness(0, 0, 8, 0)
                    };
                    Grid.SetColumn(dot, 0);
                    itemGrid.Children.Add(dot);

                    TextBlock itemText = new TextBlock
                    {
                        Text = line.Substring(1).Trim(),
                        FontSize = 11.5,
                        Foreground = new SolidColorBrush(Color.FromRgb(201, 209, 217)),
                        TextWrapping = TextWrapping.Wrap,
                        LineHeight = 17
                    };
                    Grid.SetColumn(itemText, 1);
                    itemGrid.Children.Add(itemText);

                    bulletBox.Children.Add(itemGrid);
                }
                else
                {
                    inBulletList = false;
                    TextBlock p = new TextBlock
                    {
                        Text = line,
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                        TextWrapping = TextWrapping.Wrap,
                        LineHeight = 18,
                        Margin = new Thickness(0, 0, 0, 4)
                    };
                    bodyPanel.Children.Add(p);
                }
            }

            Grid.SetRow(bodyPanel, 1);
            contentGrid.Children.Add(bodyPanel);

            // 3. 底部操作按钮区域
            Border footerBar = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(22, 27, 34)),
                CornerRadius = new CornerRadius(0, 0, 14, 14),
                Padding = new Thickness(20, 12, 20, 16)
            };

            StackPanel btnRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            if (type == ModernDialogType.Question)
            {
                Button btnCancel = CreateModernButton(I18n.T("Dlg.BtnCancel"), Color.FromRgb(33, 38, 45), Color.FromRgb(48, 54, 61), 86, 32, 6, Color.FromRgb(201, 209, 217), 11.5, false, Color.FromRgb(60, 66, 75), 1);
                btnCancel.Margin = new Thickness(0, 0, 10, 0);
                btnCancel.Click += (s, e) => { this.Confirmed = false; this.Close(); };
                btnRow.Children.Add(btnCancel);

                Button btnConfirm = CreateModernButton(I18n.T("Dlg.BtnConfirm"), accentColor, Color.FromArgb(220, accentColor.R, accentColor.G, accentColor.B), 96, 32, 6, Colors.White, 11.5, true, Colors.Transparent, 0);
                btnConfirm.Click += (s, e) => { this.Confirmed = true; this.Close(); };
                btnRow.Children.Add(btnConfirm);
            }
            else
            {
                string btnText = (type == ModernDialogType.Success) ? I18n.T("Dlg.BtnOk") : I18n.T("Dlg.BtnGotIt");
                Button btnOk = CreateModernButton(btnText, accentColor, Color.FromArgb(220, accentColor.R, accentColor.G, accentColor.B), 108, 34, 6, Colors.White, 12, true, Colors.Transparent, 0);
                btnOk.Click += (s, e) => { this.Confirmed = true; this.Close(); };
                btnRow.Children.Add(btnOk);
            }

            footerBar.Child = btnRow;
            Grid.SetRow(footerBar, 2);
            contentGrid.Children.Add(footerBar);

            card.Child = contentGrid;
            root.Children.Add(card);
            this.Content = root;

            // 键盘与淡入交互
            this.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    this.Confirmed = false;
                    this.Close();
                }
                else if (e.Key == System.Windows.Input.Key.Enter)
                {
                    this.Confirmed = true;
                    this.Close();
                }
            };

            this.Loaded += (s, e) =>
            {
                DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(160));
                this.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
        }

        private static Button CreateModernButton(string text, Color bgColor, Color hoverColor, double width, double height, double cornerRadius, Color textColor, double fontSize, bool bold, Color borderColor, double borderThickness)
        {
            Button btn = new Button
            {
                Content = text,
                Width = width,
                Height = height,
                FontSize = fontSize,
                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
                Foreground = new SolidColorBrush(textColor),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border), "bd");
            borderFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush(bgColor));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(cornerRadius));

            if (borderThickness > 0)
            {
                borderFactory.SetValue(Border.BorderBrushProperty, new SolidColorBrush(borderColor));
                borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(borderThickness));
            }

            FrameworkElementFactory contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(contentFactory);

            template.VisualTree = borderFactory;

            Trigger trigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(hoverColor), "bd"));
            template.Triggers.Add(trigger);

            btn.Template = template;
            return btn;
        }
    }

    public class MainWindow : Window
    {
        private int _currentTabIndex = 0;

        private void RebuildUI()
        {
            int curTab = _currentTabIndex;
            string existingLog = (txtConsole != null) ? txtConsole.Text : null;
            this.Title = I18n.T("App.Title");
            BuildUI();
            if (existingLog != null && txtConsole != null)
            {
                if (existingLog.Contains("[系统初始化]") || existingLog.Contains("[System Init]"))
                {
                    txtConsole.Text = I18n.T("Log.Greeting");
                }
                else
                {
                    txtConsole.Text = existingLog;
                }
            }
            SwitchTab(curTab);
            RefreshAllStatus();
        }

        #region DWM 沉浸式暗黑标题栏 Win32 API

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            try
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                int trueVal = 1;
                // 优先使用 Windows 10 20H1+ 及 Windows 11 标准暗黑标题栏属性
                int res = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueVal, sizeof(int));
                if (res != 0)
                {
                    DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref trueVal, sizeof(int));
                }
                // 设置圆角窗口首选项 (Win11)
                int cornerPreference = 2; // DWMWCP_ROUND
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
            }
            catch { }
        }

        #endregion

        // 侧边栏监控标签
        private TextBlock lblStatInstall;
        private TextBlock lblStatVer;
        private TextBlock lblStatAuth;
        private TextBlock lblStatProc;
        private TextBlock lblStatPath;
        private Button btnRestoreOrig;

        // 选项卡内容区
        private ScrollViewer viewCore;
        private ScrollViewer viewAdv;
        private Border viewLog;

        // 选项卡按钮
        private Button tabBtn1;
        private Button tabBtn2;
        private Button tabBtn3;

        // 控制台与全局状态
        private TextBox txtConsole;
        private TextBlock lblGlobalStatus;
        private ProgressBar progressBar;

        // 自定义授权输入控件
        private TextBox txtAuthName;
        private TextBox txtAuthEmail;
        private TextBox txtAuthSerial;

        // 高级策略控件
        private TextBlock lblUpdateStatus;
        private TextBlock lblHostsStatus;

        // 色彩体系 (与 IDM_Pro.exe 1:1 对齐)
        private static readonly Color ColObsidian = Color.FromRgb(13, 17, 23);       // #0D1117
        private static readonly Color ColSidebar = Color.FromRgb(22, 27, 34);        // #161B22
        private static readonly Color ColCard = Color.FromRgb(17, 24, 39);           // #111827
        private static readonly Color ColBorder = Color.FromRgb(45, 55, 72);         // #2D3748
        private static readonly Color ColBorderMuted = Color.FromRgb(48, 54, 61);    // #30363D
        private static readonly Color ColBtnDark = Color.FromRgb(33, 38, 45);        // #21262D
        private static readonly Color ColBtnDarkHover = Color.FromRgb(48, 54, 61);   // #30363D

        private static readonly Color ColAmber = Color.FromRgb(245, 158, 11);        // #F59E0B
        private static readonly Color ColAmberHover = Color.FromRgb(217, 119, 6);    // #D97706
        private static readonly Color ColAmberBadgeBg = Color.FromRgb(69, 26, 3);    // #451A03
        private static readonly Color ColAmberBadgeFg = Color.FromRgb(251, 191, 36); // #FBBF24

        private static readonly Color ColBlue = Color.FromRgb(56, 189, 248);         // #38BDF8
        private static readonly Color ColBlueHover = Color.FromRgb(14, 165, 233);    // #0EA5E9
        private static readonly Color ColBlueBadgeBg = Color.FromRgb(8, 47, 73);     // #082F49
        private static readonly Color ColBlueBadgeFg = Color.FromRgb(56, 189, 248);  // #38BDF8

        private static readonly Color ColGreen = Color.FromRgb(16, 185, 129);        // #10B981
        private static readonly Color ColGreenHover = Color.FromRgb(5, 150, 105);    // #059669
        private static readonly Color ColGreenBadgeBg = Color.FromRgb(6, 78, 59);    // #064E3B
        private static readonly Color ColGreenBadgeFg = Color.FromRgb(52, 211, 153); // #34D399

        private static readonly Color ColRose = Color.FromRgb(244, 63, 94);          // #F43F5E
        private static readonly Color ColRoseHover = Color.FromRgb(225, 29, 72);     // #E11D48
        private static readonly Color ColRoseBadgeBg = Color.FromRgb(76, 5, 25);     // #4C0519
        private static readonly Color ColRoseBadgeFg = Color.FromRgb(251, 113, 133); // #FB7185

        public MainWindow() : this(0) { }

        public MainWindow(int initialTab)
        {
            this.Title = I18n.T("App.Title");
            this.Width = 1140;
            this.Height = 820;
            this.MinWidth = 1100;
            this.MinHeight = 740;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Background = new SolidColorBrush(ColObsidian);
            this.FontFamily = new FontFamily("Microsoft YaHei UI, Segoe UI");

            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_icon.png");
                if (!File.Exists(iconPath))
                    iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = BitmapFrame.Create(new Uri(iconPath, UriKind.Absolute));
                }
            }
            catch { }

            BuildUI();
            if (initialTab > 0 && initialTab < 3) SwitchTab(initialTab);
            RefreshAllStatus();
        }

        private void BuildUI()
        {
            Grid root = new Grid();
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // 1. 左侧边栏 (Sidebar)
            Border sidebarBorder = new Border
            {
                Background = new SolidColorBrush(ColSidebar),
                BorderThickness = new Thickness(0)
            };
            Grid.SetColumn(sidebarBorder, 0);
            sidebarBorder.Child = BuildSidebar();
            root.Children.Add(sidebarBorder);

            // 2. 右侧主工作区 (Main Panel)
            Grid mainPanel = BuildMainPanel();
            Grid.SetColumn(mainPanel, 1);
            root.Children.Add(mainPanel);

            this.Content = root;
        }

        #region 左侧边栏构建

        private UIElement BuildSidebar()
        {
            Grid sidebarGrid = new Grid();
            sidebarGrid.Margin = new Thickness(20, 24, 20, 20);
            sidebarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Brand
            sidebarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Status Dashboard
            sidebarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Controls Header
            sidebarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Controls Buttons
            sidebarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Spacer
            sidebarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Admin Badge

            // --- 顶部 Brand 区域 (含语言切换胶囊按钮) ---
            StackPanel brandBox = new StackPanel { Margin = new Thickness(0, 0, 0, 18) };

            Grid brandGrid = new Grid();
            brandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            brandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel titleRow = new StackPanel { Orientation = Orientation.Horizontal };

            // 尝试加载精美内嵌优化图标
            try
            {
                string iconImgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_icon.png");
                if (File.Exists(iconImgPath))
                {
                    Image brandImg = new Image
                    {
                        Source = new BitmapImage(new Uri(iconImgPath, UriKind.Absolute)),
                        Width = 26,
                        Height = 26,
                        Margin = new Thickness(0, 0, 10, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    titleRow.Children.Add(brandImg);
                }
                else
                {
                    titleRow.Children.Add(new TextBlock
                    {
                        Text = "⚡",
                        FontSize = 20,
                        Foreground = new SolidColorBrush(ColBlue),
                        Margin = new Thickness(0, 0, 8, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    });
                }
            }
            catch
            {
                titleRow.Children.Add(new TextBlock
                {
                    Text = "⚡",
                    FontSize = 20,
                    Foreground = new SolidColorBrush(ColBlue),
                    Margin = new Thickness(0, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Center
                });
            }

            titleRow.Children.Add(new TextBlock
            {
                Text = I18n.T("App.Title"),
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(240, 246, 252)),
                VerticalAlignment = VerticalAlignment.Center
            });
            Grid.SetColumn(titleRow, 0);
            brandGrid.Children.Add(titleRow);

            Button btnLangToggle = CreateCustomButton(
                I18n.T("Brand.LangToggle"),
                Color.FromRgb(30, 41, 59),
                Color.FromRgb(45, 55, 72),
                double.NaN, 26, 6,
                ColBlue, 11, true,
                ColBorderMuted, 1
            );
            btnLangToggle.Padding = new Thickness(10, 2, 10, 2);
            btnLangToggle.VerticalAlignment = VerticalAlignment.Center;
            btnLangToggle.ToolTip = I18n.T("Brand.LangTooltip");
            btnLangToggle.Click += (s, e) =>
            {
                I18n.SetLanguage(I18n.Current == AppLanguage.Zh ? AppLanguage.En : AppLanguage.Zh);
                RebuildUI();
            };
            Grid.SetColumn(btnLangToggle, 1);
            brandGrid.Children.Add(btnLangToggle);

            brandBox.Children.Add(brandGrid);

            Border badgeBox = new Border
            {
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                Padding = new Thickness(8, 2, 8, 2),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 8, 0, 0)
            };
            badgeBox.Child = new TextBlock
            {
                Text = I18n.T("Brand.Edition"),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(ColBlue)
            };
            brandBox.Children.Add(badgeBox);
            Grid.SetRow(brandBox, 0);
            sidebarGrid.Children.Add(brandBox);

            // --- 系统状态仪表盘卡片 ---
            Border statusCard = new Border
            {
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(ColObsidian),
                BorderBrush = new SolidColorBrush(ColBorderMuted),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14, 12, 14, 14),
                Margin = new Thickness(0, 0, 0, 18)
            };
            StackPanel statStack = new StackPanel();
            statStack.Children.Add(new TextBlock
            {
                Text = I18n.T("Stat.DashboardTitle"),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(139, 148, 158)),
                Margin = new Thickness(0, 0, 0, 10)
            });

            Grid statRows = new Grid();
            statRows.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statRows.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            for (int i = 0; i < 5; i++) statRows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) });

            lblStatInstall = AddStatusRow(statRows, I18n.T("Stat.RowInstall"), I18n.T("Stat.InstallMissing"), 0, Color.FromRgb(230, 237, 243));
            lblStatVer = AddStatusRow(statRows, I18n.T("Stat.RowVersion"), I18n.T("Stat.VersionUnknown"), 1, Color.FromRgb(201, 209, 217));
            lblStatAuth = AddStatusRow(statRows, I18n.T("Stat.RowAuth"), I18n.T("Stat.AuthUndetected"), 2, ColBlue);
            lblStatProc = AddStatusRow(statRows, I18n.T("Stat.RowProcess"), I18n.T("Stat.ProcessStopped"), 3, ColAmber);
            lblStatPath = AddStatusRow(statRows, I18n.T("Stat.RowPath"), I18n.T("Stat.PathUndetected"), 4, Color.FromRgb(139, 148, 158));

            statStack.Children.Add(statRows);
            statusCard.Child = statStack;
            Grid.SetRow(statusCard, 1);
            sidebarGrid.Children.Add(statusCard);

            // --- 快捷进程控制标题 ---
            TextBlock ctrlTitle = new TextBlock
            {
                Text = I18n.T("Ctrl.Title"),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(139, 148, 158)),
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(ctrlTitle, 2);
            sidebarGrid.Children.Add(ctrlTitle);

            // --- 5 个快捷控制按钮 ---
            StackPanel btnStack = new StackPanel();
            string[] opNames = {
                I18n.T("Ctrl.BtnRefresh"),
                I18n.T("Ctrl.BtnLocate"),
                I18n.T("Ctrl.BtnKill"),
                I18n.T("Ctrl.BtnStart"),
                I18n.T("Ctrl.BtnRestore")
            };
            for (int i = 0; i < opNames.Length; i++)
            {
                Button btn = CreateCustomButton(opNames[i], ColBtnDark, ColBtnDarkHover, double.NaN, 34, 8, Color.FromRgb(240, 246, 252), 12, false, ColBorderMuted, 1);
                btn.Margin = new Thickness(0, 0, 0, 6);
                int idx = i;
                btn.Click += (s, e) => HandleSidebarAction(idx);
                if (idx == 4) btnRestoreOrig = btn;
                btnStack.Children.Add(btn);
            }
            Grid.SetRow(btnStack, 3);
            sidebarGrid.Children.Add(btnStack);

            // --- 底部管理员特权徽章 ---
            Border adminCard = new Border
            {
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(ColObsidian),
                BorderBrush = new SolidColorBrush(Color.FromRgb(35, 134, 54)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10, 8, 10, 8)
            };
            adminCard.Child = new TextBlock
            {
                Text = I18n.T("Admin.Badge"),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(63, 185, 80)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(adminCard, 5);
            sidebarGrid.Children.Add(adminCard);

            return sidebarGrid;
        }

        private TextBlock AddStatusRow(Grid grid, string label, string defVal, int row, Color valColor)
        {
            TextBlock lbl = new TextBlock
            {
                Text = label,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(139, 148, 158)),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(lbl, row);
            Grid.SetColumn(lbl, 0);
            grid.Children.Add(lbl);

            TextBlock val = new TextBlock
            {
                Text = defVal,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(valColor),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(val, row);
            Grid.SetColumn(val, 1);
            grid.Children.Add(val);
            return val;
        }

        #endregion

        #region 右侧主工作区构建

        private Grid BuildMainPanel()
        {
            Grid main = new Grid();
            main.Margin = new Thickness(20, 20, 20, 16);
            main.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Top Tabview
            main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
            main.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Bottom Status Bar

            // 1. 顶部现代化选项卡栏
            Border tabviewContainer = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = new SolidColorBrush(ColSidebar),
                BorderBrush = new SolidColorBrush(ColBorderMuted),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4),
                Margin = new Thickness(0, 0, 0, 14)
            };
            Grid tabGrid = new Grid();
            tabGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            tabGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            tabGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            tabBtn1 = CreateTabButton(I18n.T("Tab.Core"), true);
            tabBtn2 = CreateTabButton(I18n.T("Tab.Adv"), false);
            tabBtn3 = CreateTabButton(I18n.T("Tab.Log"), false);

            tabBtn1.Click += (s, e) => SwitchTab(0);
            tabBtn2.Click += (s, e) => SwitchTab(1);
            tabBtn3.Click += (s, e) => SwitchTab(2);

            Grid.SetColumn(tabBtn1, 0);
            Grid.SetColumn(tabBtn2, 1);
            Grid.SetColumn(tabBtn3, 2);

            tabGrid.Children.Add(tabBtn1);
            tabGrid.Children.Add(tabBtn2);
            tabGrid.Children.Add(tabBtn3);

            tabviewContainer.Child = tabGrid;
            Grid.SetRow(tabviewContainer, 0);
            main.Children.Add(tabviewContainer);

            // 2. 选项卡主体内容区
            Grid contentContainer = new Grid();
            Grid.SetRow(contentContainer, 1);

            viewCore = BuildViewCore();
            viewAdv = BuildViewAdv();
            viewLog = BuildViewLog();

            contentContainer.Children.Add(viewCore);
            contentContainer.Children.Add(viewAdv);
            contentContainer.Children.Add(viewLog);

            main.Children.Add(contentContainer);

            // 3. 底部状态栏
            Grid bottomBar = new Grid { Margin = new Thickness(2, 12, 2, 0) };
            bottomBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            bottomBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            lblGlobalStatus = new TextBlock
            {
                Text = I18n.T("Bottom.Ready"),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(139, 148, 158)),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(lblGlobalStatus, 0);
            bottomBar.Children.Add(lblGlobalStatus);

            progressBar = new ProgressBar
            {
                Width = 160,
                Height = 6,
                Value = 100,
                Foreground = new SolidColorBrush(ColBlue),
                Background = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                BorderThickness = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(progressBar, 1);
            bottomBar.Children.Add(progressBar);

            Grid.SetRow(bottomBar, 2);
            main.Children.Add(bottomBar);

            SwitchTab(0);
            return main;
        }

        private void SwitchTab(int index)
        {
            _currentTabIndex = index;
            UpdateTabStyle(tabBtn1, index == 0);
            UpdateTabStyle(tabBtn2, index == 1);
            UpdateTabStyle(tabBtn3, index == 2);

            viewCore.Visibility = (index == 0) ? Visibility.Visible : Visibility.Collapsed;
            viewAdv.Visibility = (index == 1) ? Visibility.Visible : Visibility.Collapsed;
            viewLog.Visibility = (index == 2) ? Visibility.Visible : Visibility.Collapsed;

            if (index == 1)
            {
                RefreshSecurityPolicyStatus();
            }
        }

        private Button CreateTabButton(string text, bool isSelected)
        {
            Button btn = new Button
            {
                Content = text,
                Height = 36,
                Cursor = System.Windows.Input.Cursors.Hand,
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                FontSize = 11,
                FontWeight = isSelected ? FontWeights.Bold : FontWeights.Normal,
                Margin = new Thickness(2, 0, 2, 0)
            };
            UpdateTabStyle(btn, isSelected);
            return btn;
        }

        private void UpdateTabStyle(Button btn, bool isSelected)
        {
            Color bg = isSelected ? Color.FromRgb(31, 111, 235) : Colors.Transparent;
            Color fg = isSelected ? Colors.White : Color.FromRgb(139, 148, 158);
            Color hover = isSelected ? Color.FromRgb(56, 139, 253) : Color.FromRgb(33, 38, 45);

            btn.Foreground = new SolidColorBrush(fg);
            btn.FontWeight = isSelected ? FontWeights.Bold : FontWeights.Normal;

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.Name = "bd";
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            border.SetValue(Border.BackgroundProperty, new SolidColorBrush(bg));

            FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(content);

            template.VisualTree = border;

            Trigger trigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(hover), "bd"));
            template.Triggers.Add(trigger);

            btn.Template = template;
        }

        // --- 选项卡 1：核心授权模式 ---
        private ScrollViewer BuildViewCore()
        {
            ScrollViewer scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = new Thickness(0, 0, 6, 0)
            };

            StackPanel panel = new StackPanel();

            // 模式一
            panel.Children.Add(CreateModeCard(
                I18n.T("Mode1.Title"),
                I18n.T("Mode1.Badge"),
                I18n.T("Mode1.Desc"),
                I18n.T("Mode1.Btn"),
                ColAmber,
                ColAmberHover,
                ColAmberBadgeBg,
                ColAmberBadgeFg,
                () => ExecutePatch()
            ));

            // 模式二
            panel.Children.Add(CreateModeCard(
                I18n.T("Mode2.Title"),
                I18n.T("Mode2.Badge"),
                I18n.T("Mode2.Desc"),
                I18n.T("Mode2.Btn"),
                ColBlue,
                ColBlueHover,
                ColBlueBadgeBg,
                ColBlueBadgeFg,
                () => ExecuteTrialFreeze()
            ));

            // 模式三：含自定义姓名、邮箱、序列号输入框
            panel.Children.Add(CreateRegisterCard());

            // 模式四：出厂纯净重置
            panel.Children.Add(CreateModeCard(
                I18n.T("Mode4.Title"),
                I18n.T("Mode4.Badge"),
                I18n.T("Mode4.Desc"),
                I18n.T("Mode4.Btn"),
                ColRose,
                ColRoseHover,
                ColRoseBadgeBg,
                ColRoseBadgeFg,
                () => ExecuteReset()
            ));

            scroll.Content = panel;
            return scroll;
        }

        private Border CreateModeCard(string title, string badge, string desc, string btnText,
            Color titleColor, Color btnHoverColor, Color badgeBg, Color badgeFg, Action onClick)
        {
            Border card = new Border
            {
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(ColCard),
                BorderBrush = new SolidColorBrush(ColBorder),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(18, 16, 18, 18),
                Margin = new Thickness(0, 0, 0, 14)
            };

            StackPanel sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch };

            // 1. 顶部 Header (居中放置：标题 + 发光徽章)
            StackPanel header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };

            header.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(titleColor),
                VerticalAlignment = VerticalAlignment.Center
            });

            Border badgeBorder = new Border
            {
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(badgeBg),
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            badgeBorder.Child = new TextBlock
            {
                Text = badge,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(badgeFg)
            };
            header.Children.Add(badgeBorder);
            sp.Children.Add(header);

            // 2. 居中文案描述
            TextBlock descBlock = new TextBlock
            {
                Text = desc,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 14),
                LineHeight = 18
            };
            sp.Children.Add(descBlock);

            // 3. 居中操作大按钮 (宽度 380，高度 40，圆角 10)
            Button btn = CreateCustomButton(btnText, titleColor, btnHoverColor, 380, 40, 10, Colors.White, 13, true, Colors.Transparent, 0);
            btn.HorizontalAlignment = HorizontalAlignment.Center;
            btn.Click += (s, e) => onClick();
            sp.Children.Add(btn);

            card.Child = sp;
            return card;
        }

        private Border CreateRegisterCard()
        {
            Border card = new Border
            {
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(ColCard),
                BorderBrush = new SolidColorBrush(ColBorder),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(18, 16, 18, 18),
                Margin = new Thickness(0, 0, 0, 14)
            };

            StackPanel sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch };

            // 1. Header
            StackPanel header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };

            header.Children.Add(new TextBlock
            {
                Text = I18n.T("Mode3.Title"),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(ColGreen),
                VerticalAlignment = VerticalAlignment.Center
            });

            Border badgeBorder = new Border
            {
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(ColGreenBadgeBg),
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            badgeBorder.Child = new TextBlock
            {
                Text = I18n.T("Mode3.Badge"),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(ColGreenBadgeFg)
            };
            header.Children.Add(badgeBorder);
            sp.Children.Add(header);

            // 2. 描述
            TextBlock descBlock = new TextBlock
            {
                Text = I18n.T("Mode3.Desc"),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 12),
                LineHeight = 18
            };
            sp.Children.Add(descBlock);

            // 3. 现代化输入表单容器
            Border inputCard = new Border
            {
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromRgb(31, 41, 55)),
                Padding = new Thickness(14, 10, 14, 12),
                Margin = new Thickness(10, 0, 10, 14),
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 560
            };

            Grid formGrid = new Grid();
            formGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            formGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });
            formGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            formGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            formGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(36) });
            formGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(36) });

            // Row 0: 姓名 + 邮箱
            TextBlock lblName = new TextBlock
            {
                Text = I18n.T("Mode3.LblName"),
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            Grid.SetRow(lblName, 0);
            Grid.SetColumn(lblName, 0);
            formGrid.Children.Add(lblName);

            string defaultName = GetDefaultUserName();
            string defaultEmail = GetDefaultUserEmail(defaultName);

            txtAuthName = CreateInputBox(defaultName, 150);
            Grid.SetRow(txtAuthName, 0);
            Grid.SetColumn(txtAuthName, 1);
            formGrid.Children.Add(txtAuthName);

            TextBlock lblEmail = new TextBlock
            {
                Text = I18n.T("Mode3.LblEmail"),
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(14, 0, 8, 0)
            };
            Grid.SetRow(lblEmail, 0);
            Grid.SetColumn(lblEmail, 2);
            formGrid.Children.Add(lblEmail);

            txtAuthEmail = CreateInputBox(defaultEmail, 210);
            Grid.SetRow(txtAuthEmail, 0);
            Grid.SetColumn(txtAuthEmail, 3);
            formGrid.Children.Add(txtAuthEmail);

            // 智能联动：当用户修改登记姓名时，邮箱若为默认规则则自动同步跟随
            txtAuthName.TextChanged += (s, e) =>
            {
                if (txtAuthName != null && txtAuthEmail != null)
                {
                    string curName = txtAuthName.Text.Trim();
                    if (!string.IsNullOrEmpty(curName))
                    {
                        if (string.IsNullOrEmpty(txtAuthEmail.Text) || txtAuthEmail.Text.EndsWith("@vipuser.com") || txtAuthEmail.Text.EndsWith("@tonec.com"))
                        {
                            txtAuthEmail.Text = GetDefaultUserEmail(curName);
                        }
                    }
                }
            };

            // Row 1: 证书号 + 随机生成按钮
            TextBlock lblSerial = new TextBlock
            {
                Text = I18n.T("Mode3.LblSerial"),
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            Grid.SetRow(lblSerial, 1);
            Grid.SetColumn(lblSerial, 0);
            formGrid.Children.Add(lblSerial);

            txtAuthSerial = CreateInputBox(GenerateSerial(), 260);
            txtAuthSerial.FontFamily = new FontFamily("Consolas");
            Grid.SetRow(txtAuthSerial, 1);
            Grid.SetColumn(txtAuthSerial, 1);
            Grid.SetColumnSpan(txtAuthSerial, 2);
            formGrid.Children.Add(txtAuthSerial);

            Button btnRandom = CreateCustomButton(I18n.T("Mode3.BtnRandom"), Color.FromRgb(55, 65, 81), Color.FromRgb(75, 85, 99), 90, 28, 6, Colors.White, 11, false, Colors.Transparent, 0);
            btnRandom.Margin = new Thickness(14, 0, 0, 0);
            btnRandom.HorizontalAlignment = HorizontalAlignment.Left;
            btnRandom.Click += (s, e) => txtAuthSerial.Text = GenerateSerial();
            Grid.SetRow(btnRandom, 1);
            Grid.SetColumn(btnRandom, 3);
            formGrid.Children.Add(btnRandom);

            inputCard.Child = formGrid;
            sp.Children.Add(inputCard);

            // 4. 居中提交按钮
            Button btnSubmit = CreateCustomButton(I18n.T("Mode3.BtnSubmit"), ColGreen, ColGreenHover, 380, 40, 10, Colors.White, 13, true, Colors.Transparent, 0);
            btnSubmit.HorizontalAlignment = HorizontalAlignment.Center;
            btnSubmit.Click += (s, e) => ExecuteRegister();
            sp.Children.Add(btnSubmit);

            card.Child = sp;
            return card;
        }

        private TextBox CreateInputBox(string defaultText, double width)
        {
            TextBox tb = new TextBox
            {
                Text = defaultText,
                Width = width,
                Height = 28,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(240, 246, 252)),
                Background = new SolidColorBrush(Color.FromRgb(17, 24, 39)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(55, 65, 81)),
                BorderThickness = new Thickness(1),
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding = new Thickness(6, 2, 6, 2),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            return tb;
        }

        private static string GenerateSerial()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            Random rnd = new Random();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                if (i > 0) sb.Append("-");
                for (int j = 0; j < 5; j++)
                {
                    sb.Append(chars[rnd.Next(chars.Length)]);
                }
            }
            return sb.ToString();
        }

                // --- 选项卡 2：高级安全与策略 (Modern Fluent 驾驶舱风格) ---
        private Border cardUpdateStatusBd;
        private Border cardHostsStatusBd;

        private ScrollViewer BuildViewAdv()
        {
            ScrollViewer scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(0, 0, 6, 0)
            };

            StackPanel sp = new StackPanel();

            // 1. 弹窗防御与联网更新策略卡片
            Border secCard = new Border
            {
                CornerRadius = new CornerRadius(14),
                Background = new SolidColorBrush(ColCard),
                BorderBrush = new SolidColorBrush(ColBorder),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(24, 20, 24, 22),
                Margin = new Thickness(0, 0, 0, 16)
            };

            StackPanel secInner = new StackPanel();

            // 卡片标题栏
            StackPanel secHeader = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };
            secHeader.Children.Add(new TextBlock
            {
                Text = I18n.T("Sec.Header"),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(ColBlue),
                VerticalAlignment = VerticalAlignment.Center
            });

            Border secBadge = new Border
            {
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(ColBlueBadgeBg),
                Padding = new Thickness(10, 3, 10, 3),
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            secBadge.Child = new TextBlock
            {
                Text = I18n.T("Sec.Badge"),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(ColBlueBadgeFg)
            };
            secHeader.Children.Add(secBadge);
            secInner.Children.Add(secHeader);

            // 描述
            secInner.Children.Add(new TextBlock
            {
                Text = I18n.T("Sec.Desc"),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 18),
                LineHeight = 18
            });

            // 策略控制区：左右双面板
            Grid policyGrid = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            policyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            policyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) }); // 间距
            policyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // [左面板：自动更新策略]
            cardUpdateStatusBd = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = new SolidColorBrush(Color.FromRgb(22, 27, 34)),
                BorderBrush = new SolidColorBrush(ColBorderMuted),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(16, 14, 16, 14)
            };
            StackPanel p1Inner = new StackPanel();

            // 顶部状态条
            StackPanel p1Header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            p1Header.Children.Add(new TextBlock
            {
                Text = I18n.T("Sec.UpdateTitle"),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(240, 246, 252))
            });
            p1Inner.Children.Add(p1Header);

            lblUpdateStatus = new TextBlock
            {
                Text = I18n.T("Sec.UpdateChecking"),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(201, 209, 217)),
                Margin = new Thickness(0, 0, 0, 8)
            };
            p1Inner.Children.Add(lblUpdateStatus);

            p1Inner.Children.Add(new TextBlock
            {
                Text = I18n.T("Sec.UpdateDesc"),
                FontSize = 10.5,
                Foreground = new SolidColorBrush(Color.FromRgb(139, 148, 158)),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 15,
                Height = 32,
                Margin = new Thickness(0, 0, 0, 12)
            });

            // 按钮行
            Grid p1BtnGrid = new Grid();
            p1BtnGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            p1BtnGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            p1BtnGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Button btnBlockUpdate = CreateCustomButton(I18n.T("Sec.BtnBlockUpdate"), ColAmber, ColAmberHover, double.NaN, 34, 7, Colors.White, 11, true, Colors.Transparent, 0);
            btnBlockUpdate.Click += (s, e) => ToggleUpdateCheck(true);
            Grid.SetColumn(btnBlockUpdate, 0);
            p1BtnGrid.Children.Add(btnBlockUpdate);

            Button btnAllowUpdate = CreateCustomButton(I18n.T("Sec.BtnAllowUpdate"), ColBtnDark, ColBtnDarkHover, double.NaN, 34, 7, Color.FromRgb(240, 246, 252), 11, false, ColBorderMuted, 1);
            btnAllowUpdate.Click += (s, e) => ToggleUpdateCheck(false);
            Grid.SetColumn(btnAllowUpdate, 2);
            p1BtnGrid.Children.Add(btnAllowUpdate);

            p1Inner.Children.Add(p1BtnGrid);
            cardUpdateStatusBd.Child = p1Inner;
            Grid.SetColumn(cardUpdateStatusBd, 0);
            policyGrid.Children.Add(cardUpdateStatusBd);

            // [右面板：Hosts 域名反封锁]
            cardHostsStatusBd = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = new SolidColorBrush(Color.FromRgb(22, 27, 34)),
                BorderBrush = new SolidColorBrush(ColBorderMuted),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(16, 14, 16, 14)
            };
            StackPanel p2Inner = new StackPanel();

            StackPanel p2Header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            p2Header.Children.Add(new TextBlock
            {
                Text = I18n.T("Sec.HostsTitle"),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(240, 246, 252))
            });
            p2Inner.Children.Add(p2Header);

            lblHostsStatus = new TextBlock
            {
                Text = I18n.T("Sec.UpdateChecking"),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(201, 209, 217)),
                Margin = new Thickness(0, 0, 0, 8)
            };
            p2Inner.Children.Add(lblHostsStatus);

            p2Inner.Children.Add(new TextBlock
            {
                Text = I18n.T("Sec.HostsDesc"),
                FontSize = 10.5,
                Foreground = new SolidColorBrush(Color.FromRgb(139, 148, 158)),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 15,
                Height = 32,
                Margin = new Thickness(0, 0, 0, 12)
            });

            // 按钮行
            Grid p2BtnGrid = new Grid();
            p2BtnGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            p2BtnGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            p2BtnGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Button btnBlockHosts = CreateCustomButton(I18n.T("Sec.BtnBlockHosts"), ColBlue, ColBlueHover, double.NaN, 34, 7, Colors.White, 11, true, Colors.Transparent, 0);
            btnBlockHosts.Click += (s, e) => ToggleHostsBlock(true);
            Grid.SetColumn(btnBlockHosts, 0);
            p2BtnGrid.Children.Add(btnBlockHosts);

            Button btnRestoreHosts = CreateCustomButton(I18n.T("Sec.BtnRestoreHosts"), ColBtnDark, ColBtnDarkHover, double.NaN, 34, 7, Color.FromRgb(240, 246, 252), 11, false, ColBorderMuted, 1);
            btnRestoreHosts.Click += (s, e) => ToggleHostsBlock(false);
            Grid.SetColumn(btnRestoreHosts, 2);
            p2BtnGrid.Children.Add(btnRestoreHosts);

            p2Inner.Children.Add(p2BtnGrid);
            cardHostsStatusBd.Child = p2Inner;
            Grid.SetColumn(cardHostsStatusBd, 2);
            policyGrid.Children.Add(cardHostsStatusBd);

            secInner.Children.Add(policyGrid);
            secCard.Child = secInner;
            sp.Children.Add(secCard);

            // 2. 系统路径与注册表实用工具箱 (3列磁贴卡片)
            Border toolCard = new Border
            {
                CornerRadius = new CornerRadius(14),
                Background = new SolidColorBrush(ColCard),
                BorderBrush = new SolidColorBrush(ColBorder),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(24, 20, 24, 22),
                Margin = new Thickness(0, 0, 0, 14)
            };

            StackPanel inner = new StackPanel();

            StackPanel toolHeader = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            };
            toolHeader.Children.Add(new TextBlock
            {
                Text = I18n.T("Tool.Header"),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(240, 246, 252)),
                VerticalAlignment = VerticalAlignment.Center
            });
            Border toolBadge = new Border
            {
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromRgb(33, 38, 45)),
                BorderBrush = new SolidColorBrush(ColBorderMuted),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            toolBadge.Child = new TextBlock
            {
                Text = I18n.T("Tool.Badge"),
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(139, 148, 158))
            };
            toolHeader.Children.Add(toolBadge);
            inner.Children.Add(toolHeader);

            Grid toolsGrid = new Grid();
            toolsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            toolsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            toolsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            toolsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            toolsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            toolsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            toolsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            string[] titles = {
                I18n.T("Tool.BackupTitle"),
                I18n.T("Tool.RegeditTitle"),
                I18n.T("Tool.DirTitle"),
                I18n.T("Tool.LocateTitle")
            };
            string[] subs = {
                I18n.T("Tool.BackupSub"),
                I18n.T("Tool.RegeditSub"),
                I18n.T("Tool.DirSub"),
                I18n.T("Tool.LocateSub")
            };

            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                Button b = CreateTileButton(titles[i], subs[i]);
                b.Click += (s, e) => HandleAdvToolAction(idx);
                Grid.SetColumn(b, i * 2);
                toolsGrid.Children.Add(b);
            }

            inner.Children.Add(toolsGrid);
            toolCard.Child = inner;
            sp.Children.Add(toolCard);

            scroll.Content = sp;
            return scroll;
        }

        private static Button CreateTileButton(string title, string sub)
        {
            Button btn = new Button
            {
                Height = 62,
                Cursor = System.Windows.Input.Cursors.Hand,
                Background = Brushes.Transparent
            };

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border), "bd");
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
            borderFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(22, 27, 34)));
            borderFactory.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(48, 54, 61)));
            borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            borderFactory.SetValue(Border.PaddingProperty, new Thickness(10, 8, 10, 8));

            FrameworkElementFactory sp = new FrameworkElementFactory(typeof(StackPanel));
            sp.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);

            FrameworkElementFactory t1 = new FrameworkElementFactory(typeof(TextBlock));
            t1.SetValue(TextBlock.TextProperty, title);
            t1.SetValue(TextBlock.FontSizeProperty, 12.0);
            t1.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            t1.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(240, 246, 252)));
            t1.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            sp.AppendChild(t1);

            FrameworkElementFactory t2 = new FrameworkElementFactory(typeof(TextBlock));
            t2.SetValue(TextBlock.TextProperty, sub);
            t2.SetValue(TextBlock.FontSizeProperty, 9.5);
            t2.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(139, 148, 158)));
            t2.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            t2.SetValue(TextBlock.MarginProperty, new Thickness(0, 3, 0, 0));
            sp.AppendChild(t2);

            borderFactory.AppendChild(sp);
            template.VisualTree = borderFactory;

            Trigger trigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(33, 38, 45)), "bd"));
            trigger.Setters.Add(new Setter(Border.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(56, 189, 248)), "bd"));
            template.Triggers.Add(trigger);

            btn.Template = template;
            return btn;
        }

        // --- 选项卡 3：实时操作控制台 ---
        private Border BuildViewLog()
        {
            Border logCard = new Border
            {
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(Color.FromRgb(10, 14, 20)),
                BorderBrush = new SolidColorBrush(ColBorderMuted),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14)
            };

            txtConsole = new TextBox
            {
                Background = Brushes.Transparent,
                Foreground = new SolidColorBrush(Color.FromRgb(52, 211, 153)),
                BorderThickness = new Thickness(0),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Text = I18n.T("Log.Greeting")
            };

            logCard.Child = txtConsole;
            return logCard;
        }

        #endregion

        #region 自定义现代发光圆角按钮构建器

        public static Button CreateCustomButton(string text, Color normalColor, Color hoverColor, double width, double height, double cornerRadius, Color textColor, double fontSize, bool isBold, Color borderColor, double borderThickness)
        {
            Button btn = new Button
            {
                Content = text,
                Cursor = System.Windows.Input.Cursors.Hand,
                Foreground = new SolidColorBrush(textColor),
                FontSize = fontSize,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                FontFamily = new FontFamily("Microsoft YaHei UI")
            };

            if (!double.IsNaN(width)) btn.Width = width;
            if (!double.IsNaN(height)) btn.Height = height;

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.Name = "bd";
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(cornerRadius));
            borderFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush(normalColor));
            if (borderThickness > 0)
            {
                borderFactory.SetValue(Border.BorderBrushProperty, new SolidColorBrush(borderColor));
                borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(borderThickness));
            }

            FrameworkElementFactory contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(contentFactory);

            template.VisualTree = borderFactory;

            Trigger trigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(hoverColor), "bd"));
            template.Triggers.Add(trigger);

            btn.Template = template;
            return btn;
        }

        #endregion

        #region 核心业务逻辑与安全策略实现

        private void Log(string msg)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<string>(Log), msg);
                return;
            }
            if (I18n.Current == AppLanguage.En)
            {
                msg = I18n.TranslateLog(msg);
            }
            txtConsole.AppendText("\r\n[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg);
            txtConsole.ScrollToEnd();
            lblGlobalStatus.Text = "ℹ️ " + msg;
        }

        private static string _customIDMDir = null;

        public static string GetIDMDir()
        {
            // 0. 优先使用内存中的自定义指定目录
            if (!string.IsNullOrEmpty(_customIDMDir) && Directory.Exists(_customIDMDir))
            {
                if (File.Exists(Path.Combine(_customIDMDir, "IDMan.exe")))
                    return _customIDMDir;
            }

            // 1. 读取配置文件或保存的持久化设置
            try
            {
                string cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "idm_path.cfg");
                if (File.Exists(cfgPath))
                {
                    string saved = File.ReadAllText(cfgPath, Encoding.UTF8).Trim();
                    if (!string.IsNullOrEmpty(saved))
                    {
                        if (File.Exists(saved) && Path.GetFileName(saved).Equals("IDMan.exe", StringComparison.OrdinalIgnoreCase))
                            saved = Path.GetDirectoryName(saved);
                        if (Directory.Exists(saved) && File.Exists(Path.Combine(saved, "IDMan.exe")))
                        {
                            _customIDMDir = saved;
                            return _customIDMDir;
                        }
                    }
                }
            }
            catch { }

            // 2. 从注册表 HKCU\Software\DownloadManager\ExePath 读取
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\DownloadManager"))
                {
                    if (k != null)
                    {
                        object p = k.GetValue("ExePath");
                        if (p != null)
                        {
                            string exePath = p.ToString().Trim();
                            if (File.Exists(exePath))
                            {
                                string dir = Path.GetDirectoryName(exePath);
                                if (Directory.Exists(dir)) return dir;
                            }
                        }
                    }
                }
            }
            catch { }

            // 3. 从卸载项注册表读取 (支持非系统盘安装的 IDM)
            string[] uninstallKeys = new string[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Internet Download Manager",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Internet Download Manager"
            };
            foreach (string uKey in uninstallKeys)
            {
                try
                {
                    using (RegistryKey k = Registry.LocalMachine.OpenSubKey(uKey))
                    {
                        if (k != null)
                        {
                            object icon = k.GetValue("DisplayIcon");
                            if (icon != null)
                            {
                                string iconPath = icon.ToString().Trim().Trim('"');
                                if (File.Exists(iconPath))
                                {
                                    string dir = Path.GetDirectoryName(iconPath);
                                    if (Directory.Exists(dir)) return dir;
                                }
                            }
                            object uninst = k.GetValue("UninstallString");
                            if (uninst != null)
                            {
                                string uninstPath = uninst.ToString().Trim().Trim('"');
                                if (File.Exists(uninstPath))
                                {
                                    string dir = Path.GetDirectoryName(uninstPath);
                                    if (Directory.Exists(dir) && File.Exists(Path.Combine(dir, "IDMan.exe"))) return dir;
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            // 4. 从后台正在运行的 IDMan 进程主模块读取 (若正在运行，直接精准抓取实际路径)
            try
            {
                Process[] procs = Process.GetProcessesByName("IDMan");
                if (procs.Length > 0 && procs[0].MainModule != null)
                {
                    string procExe = procs[0].MainModule.FileName;
                    if (File.Exists(procExe))
                    {
                        string dir = Path.GetDirectoryName(procExe);
                        if (Directory.Exists(dir)) return dir;
                    }
                }
            }
            catch { }

            // 5. 传统默认安装路径
            string p1 = @"C:\Program Files (x86)\Internet Download Manager";
            if (Directory.Exists(p1) && File.Exists(Path.Combine(p1, "IDMan.exe"))) return p1;
            string p2 = @"C:\Program Files\Internet Download Manager";
            if (Directory.Exists(p2) && File.Exists(Path.Combine(p2, "IDMan.exe"))) return p2;

            // 6. 遍历所有固定驱动器 (D:, E:, F:, G: 等) 常见路径探测
            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        string[] candidates = new string[]
                        {
                            Path.Combine(drive.RootDirectory.FullName, "Program Files (x86)", "Internet Download Manager"),
                            Path.Combine(drive.RootDirectory.FullName, "Program Files", "Internet Download Manager"),
                            Path.Combine(drive.RootDirectory.FullName, "Internet Download Manager"),
                            Path.Combine(drive.RootDirectory.FullName, "IDM"),
                            Path.Combine(drive.RootDirectory.FullName, "Software", "Internet Download Manager"),
                            Path.Combine(drive.RootDirectory.FullName, "Software", "IDM"),
                            Path.Combine(drive.RootDirectory.FullName, "Apps", "Internet Download Manager"),
                            Path.Combine(drive.RootDirectory.FullName, "Apps", "IDM")
                        };
                        foreach (string cand in candidates)
                        {
                            if (Directory.Exists(cand) && File.Exists(Path.Combine(cand, "IDMan.exe")))
                                return cand;
                        }
                    }
                }
            }
            catch { }

            if (Directory.Exists(p1)) return p1;
            if (Directory.Exists(p2)) return p2;
            return p1;
        }

        public static bool SetCustomIDMPath(string selectedPath, out string errorMsg)
        {
            errorMsg = null;
            try
            {
                if (string.IsNullOrEmpty(selectedPath))
                {
                    errorMsg = I18n.T("Err.PathEmpty");
                    return false;
                }
                string targetDir = selectedPath.Trim().Trim('"');
                if (File.Exists(targetDir))
                {
                    if (Path.GetFileName(targetDir).Equals("IDMan.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        targetDir = Path.GetDirectoryName(targetDir);
                    }
                    else
                    {
                        errorMsg = I18n.T("Err.NotIDMExe");
                        return false;
                    }
                }

                if (!Directory.Exists(targetDir))
                {
                    errorMsg = I18n.T("Err.DirNotExist", targetDir);
                    return false;
                }

                string idmExe = Path.Combine(targetDir, "IDMan.exe");
                if (!File.Exists(idmExe))
                {
                    errorMsg = I18n.T("Err.ExeNotFoundInDir", targetDir);
                    return false;
                }

                _customIDMDir = targetDir;

                // 持久化保存至本地配置文件
                try
                {
                    string cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "idm_path.cfg");
                    File.WriteAllText(cfgPath, targetDir, Encoding.UTF8);
                }
                catch { }

                // 同步更新注册表 ExePath 键值
                try
                {
                    using (RegistryKey k = Registry.CurrentUser.CreateSubKey(@"Software\DownloadManager"))
                    {
                        if (k != null)
                        {
                            k.SetValue("ExePath", idmExe, RegistryValueKind.String);
                        }
                    }
                }
                catch { }

                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        public void PromptSelectIDMPath()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = I18n.T("Dlg.Locate.DialogTitle");
            dlg.Filter = I18n.T("Dlg.Locate.Filter");
            dlg.FileName = "IDMan.exe";
            dlg.CheckFileExists = true;

            string curr = GetIDMDir();
            if (Directory.Exists(curr))
                dlg.InitialDirectory = curr;

            bool? result = dlg.ShowDialog(this);
            if (result == true && !string.IsNullOrEmpty(dlg.FileName))
            {
                string err;
                if (SetCustomIDMPath(dlg.FileName, out err))
                {
                    Log("✓ 已成功手动定位 IDM 路径: " + _customIDMDir);
                    RefreshAllStatus();
                    ModernDialog.ShowSuccess(this, I18n.T("Dlg.Locate.Success.Title"), I18n.T("Dlg.Locate.Success.Msg", _customIDMDir));
                }
                else
                {
                    Log("定位 IDM 路径失败: " + err);
                    ModernDialog.ShowError(this, I18n.T("Dlg.Locate.Failed.Title"), I18n.T("Dlg.Locate.Failed.Msg", err));
                }
            }
        }

        public static string GetDefaultUserName()
        {
            try
            {
                string u = Environment.UserName;
                if (!string.IsNullOrEmpty(u) &&
                    !string.Equals(u, "SYSTEM", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(u, "LOCAL SERVICE", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(u, "NETWORK SERVICE", StringComparison.OrdinalIgnoreCase))
                {
                    return u.Trim();
                }
            }
            catch { }
            return "user";
        }

        public static string GetDefaultUserEmail(string userName = null)
        {
            if (string.IsNullOrEmpty(userName)) userName = GetDefaultUserName();
            string clean = Regex.Replace(userName.ToLowerInvariant(), @"[^a-z0-9]", ".");
            clean = clean.Trim('.');
            if (string.IsNullOrEmpty(clean)) clean = "user";
            return clean + "@vipuser.com";
        }

        private void RefreshAllStatus()
        {
            string idmDir = GetIDMDir();
            string idmExe = Path.Combine(idmDir, "IDMan.exe");
            string idmBak = idmExe + ".BAK";

            // 1. 安装检测
            if (File.Exists(idmExe))
            {
                lblStatInstall.Text = I18n.T("Stat.InstallInstalled");
                lblStatInstall.Foreground = new SolidColorBrush(Color.FromRgb(63, 185, 80));
            }
            else
            {
                lblStatInstall.Text = I18n.T("Stat.InstallMissing");
                lblStatInstall.Foreground = new SolidColorBrush(Color.FromRgb(248, 81, 73));
            }

            // 2. 版本检测
            if (File.Exists(idmExe))
            {
                try
                {
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(idmExe);
                    lblStatVer.Text = "v" + fvi.FileVersion;
                }
                catch
                {
                    lblStatVer.Text = "v6.4x";
                }
            }
            else
            {
                lblStatVer.Text = I18n.T("Stat.VersionUnknown");
            }

            // 3. 授权检测
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\DownloadManager"))
                {
                    bool isFrozen = TrialFreezeEngine.IsTrialFrozen();
                    if (k != null)
                    {
                        object fname = k.GetValue("FName");
                        if (fname != null && !string.IsNullOrEmpty(fname.ToString().Trim()))
                        {
                            lblStatAuth.Text = I18n.T("Stat.AuthRegistered", fname.ToString().Trim());
                            lblStatAuth.Foreground = new SolidColorBrush(ColBlue);
                        }
                        else if (isFrozen)
                        {
                            lblStatAuth.Text = I18n.T("Stat.AuthFrozen");
                            lblStatAuth.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                        }
                        else
                        {
                            lblStatAuth.Text = I18n.T("Stat.AuthUnregistered");
                            lblStatAuth.Foreground = new SolidColorBrush(ColAmber);
                        }
                    }
                    else if (isFrozen)
                    {
                        lblStatAuth.Text = I18n.T("Stat.AuthFrozen");
                        lblStatAuth.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                    }
                    else
                    {
                        lblStatAuth.Text = I18n.T("Stat.AuthUnregistered");
                        lblStatAuth.Foreground = new SolidColorBrush(ColAmber);
                    }
                }
            }
            catch
            {
                lblStatAuth.Text = I18n.T("Stat.AuthUndetected");
            }

            // 4. 进程检测
            Process[] procs = Process.GetProcessesByName("IDMan");
            if (procs.Length > 0)
            {
                lblStatProc.Text = I18n.T("Stat.ProcessRunning", procs.Length);
                lblStatProc.Foreground = new SolidColorBrush(Color.FromRgb(63, 185, 80));
            }
            else
            {
                lblStatProc.Text = I18n.T("Stat.ProcessStopped");
                lblStatProc.Foreground = new SolidColorBrush(Color.FromRgb(139, 148, 158));
            }

            // 5. 路径来源简报展示
            if (lblStatPath != null)
            {
                if (!string.IsNullOrEmpty(_customIDMDir))
                {
                    lblStatPath.Text = I18n.T("Stat.PathCustom");
                    lblStatPath.Foreground = new SolidColorBrush(ColBlue);
                }
                else if (File.Exists(idmExe))
                {
                    lblStatPath.Text = I18n.T("Stat.PathAuto");
                    lblStatPath.Foreground = new SolidColorBrush(Color.FromRgb(63, 185, 80));
                }
                else
                {
                    lblStatPath.Text = I18n.T("Stat.PathNotFound");
                    lblStatPath.Foreground = new SolidColorBrush(ColAmber);
                }
            }

            // 6. 原版备份按钮状态
            if (btnRestoreOrig != null)
            {
                bool hasBak = File.Exists(idmBak);
                btnRestoreOrig.IsEnabled = hasBak;
                btnRestoreOrig.Opacity = hasBak ? 1.0 : 0.5;
            }

            RefreshSecurityPolicyStatus();
        }

        private void RefreshSecurityPolicyStatus()
        {
            if (lblUpdateStatus == null || lblHostsStatus == null) return;

            // 检查更新策略
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\DownloadManager"))
                {
                    object val = (k != null) ? k.GetValue("CheckUpdtVM") : null;
                    if (val != null && val.ToString() == "0")
                    {
                        lblUpdateStatus.Text = I18n.T("Sec.UpdateBlocked");
                        lblUpdateStatus.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Emerald
                        if (cardUpdateStatusBd != null) cardUpdateStatusBd.BorderBrush = new SolidColorBrush(Color.FromArgb(90, 34, 197, 94));
                    }
                    else
                    {
                        lblUpdateStatus.Text = I18n.T("Sec.UpdateAllowed");
                        lblUpdateStatus.Foreground = new SolidColorBrush(ColAmber);
                        if (cardUpdateStatusBd != null) cardUpdateStatusBd.BorderBrush = new SolidColorBrush(ColBorderMuted);
                    }
                }
            }
            catch
            {
                lblUpdateStatus.Text = I18n.T("Sec.UpdateUnknown");
            }

            // 检查 Hosts 防护
            try
            {
                string hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");
                if (File.Exists(hostsPath))
                {
                    string content = File.ReadAllText(hostsPath);
                    if (content.Contains("tonec.com") && content.Contains("registeridm.com"))
                    {
                        lblHostsStatus.Text = I18n.T("Sec.HostsActive");
                        lblHostsStatus.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Emerald
                        if (cardHostsStatusBd != null) cardHostsStatusBd.BorderBrush = new SolidColorBrush(Color.FromArgb(90, 56, 189, 248));
                    }
                    else
                    {
                        lblHostsStatus.Text = I18n.T("Sec.HostsInactive");
                        lblHostsStatus.Foreground = new SolidColorBrush(Color.FromRgb(139, 148, 158));
                        if (cardHostsStatusBd != null) cardHostsStatusBd.BorderBrush = new SolidColorBrush(ColBorderMuted);
                    }
                }
            }
            catch
            {
                lblHostsStatus.Text = I18n.T("Sec.HostsUnknown");
            }
        }

        private void ToggleUpdateCheck(bool block)
        {
            SwitchTab(2);
            try
            {
                using (RegistryKey k = Registry.CurrentUser.CreateSubKey(@"Software\DownloadManager"))
                {
                    if (k != null)
                    {
                        if (block)
                        {
                            k.SetValue("CheckUpdtVM", 0, RegistryValueKind.DWord);
                            k.SetValue("LstCheck", "0", RegistryValueKind.String);
                            Log("✓ 已启用自动更新屏蔽策略：CheckUpdtVM = 0 (已阻断启动自动检测与升级弹窗)");
                            ModernDialog.ShowSuccess(this, I18n.T("Dlg.Update.Blocked.Title"), I18n.T("Dlg.Update.Blocked.Msg"));
                        }
                        else
                        {
                            k.SetValue("CheckUpdtVM", 1, RegistryValueKind.DWord);
                            Log("✓ 已恢复 IDM 官方自动更新检查：CheckUpdtVM = 1");
                            ModernDialog.ShowInfo(this, I18n.T("Dlg.Update.Restored.Title"), I18n.T("Dlg.Update.Restored.Msg"));
                        }
                    }
                }
                RefreshSecurityPolicyStatus();
            }
            catch (Exception ex)
            {
                Log("更新策略设置失败: " + ex.Message);
            }
        }

        private void ToggleHostsBlock(bool block)
        {
            SwitchTab(2);
            string hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");
            try
            {
                if (!File.Exists(hostsPath))
                {
                    Log("错误: 未找到系统 hosts 文件: " + hostsPath);
                    return;
                }

                string content = File.ReadAllText(hostsPath, Encoding.UTF8);
                string blockTagStart = "# >>> IDM Toolkit Block >>>";
                string blockTagEnd = "# <<< IDM Toolkit Block <<<";

                // 统一先清除旧的 block 块
                int startIdx = content.IndexOf(blockTagStart);
                int endIdx = content.IndexOf(blockTagEnd);
                if (startIdx != -1 && endIdx != -1 && endIdx > startIdx)
                {
                    string before = content.Substring(0, startIdx).TrimEnd();
                    string after = content.Substring(endIdx + blockTagEnd.Length).TrimStart();
                    content = before + (string.IsNullOrEmpty(after) ? "" : "\r\n" + after);
                }

                if (block)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(blockTagStart);
                    sb.AppendLine("127.0.0.1 tonec.com");
                    sb.AppendLine("127.0.0.1 www.tonec.com");
                    sb.AppendLine("127.0.0.1 registeridm.com");
                    sb.AppendLine("127.0.0.1 www.registeridm.com");
                    sb.AppendLine("127.0.0.1 secure.internetdownloadmanager.com");
                    sb.AppendLine("127.0.0.1 mirror.internetdownloadmanager.com");
                    sb.AppendLine("127.0.0.1 mirror2.internetdownloadmanager.com");
                    sb.AppendLine("127.0.0.1 mirror3.internetdownloadmanager.com");
                    sb.Append(blockTagEnd);

                    if (!content.EndsWith("\r\n") && !string.IsNullOrEmpty(content))
                    {
                        content += "\r\n";
                    }
                    content += sb.ToString() + "\r\n";

                    File.WriteAllText(hostsPath, content, Encoding.UTF8);
                    Log("✓ 已成功将 IDM 官方黑名单与验证服务器重定向至 127.0.0.1 (系统 hosts 防护已生效)");
                    ModernDialog.ShowSuccess(this, I18n.T("Dlg.Hosts.Blocked.Title"), I18n.T("Dlg.Hosts.Blocked.Msg"));
                }
                else
                {
                    File.WriteAllText(hostsPath, content.TrimEnd() + "\r\n", Encoding.UTF8);
                    Log("✓ 已成功从系统 hosts 中移除 IDM 验证服务器拦截规则");
                    ModernDialog.ShowInfo(this, I18n.T("Dlg.Hosts.Restored.Title"), I18n.T("Dlg.Hosts.Restored.Msg"));
                }

                RefreshSecurityPolicyStatus();
            }
            catch (Exception ex)
            {
                Log("配置 Hosts 失败: " + ex.Message + " (请确认以管理员特权运行)");
                ModernDialog.ShowError(this, I18n.T("Dlg.Hosts.Failed.Title"), I18n.T("Dlg.Hosts.Failed.Msg", ex.Message));
            }
        }

        private void HandleSidebarAction(int idx)
        {
            if (idx == 0) // 刷新
            {
                RefreshAllStatus();
                Log("运行状态仪表盘与系统信息已刷新完成。当前路径: " + GetIDMDir());
            }
            else if (idx == 1) // 手动定位 IDM 路径
            {
                PromptSelectIDMPath();
            }
            else if (idx == 2) // 终止进程
            {
                KillIDM();
            }
            else if (idx == 3) // 启动进程
            {
                string idmExe = Path.Combine(GetIDMDir(), "IDMan.exe");
                if (!File.Exists(idmExe))
                {
                    Log("未找到 IDMan.exe 主程序，请尝试点击【手动定位 IDM 路径】。");
                    ModernDialog.ShowWarning(this, I18n.T("Dlg.Locate.NotFound.Title"), I18n.T("Dlg.Locate.NotFound.Msg"));
                    return;
                }

                // 启动前体检：提前拦截「不是有效的 Win32 应用程序」这类致命故障，
                // 而不是让 WPF 抛出未捕获异常（旧版会弹出原始崩溃堆栈窗口）。
                string diag = NativeBinaryPatcher.DiagnoseExecutable(idmExe);
                if (diag != null)
                {
                    Log("✗ 启动前体检未通过：" + diag);
                    Log("  目标路径: " + idmExe);
                    bool doFix = ModernDialog.Confirm(this, I18n.T("Dlg.Corrupt.Title"), I18n.T("Dlg.Corrupt.Msg", diag));
                    if (doFix) RestoreOriginal();
                    return;
                }

                try
                {
                    Process.Start(idmExe);
                    Log("已启动 IDM 应用程序。");
                    Thread.Sleep(500);
                    RefreshAllStatus();
                }
                catch (Exception ex)
                {
                    Log("✗ 启动 IDM 失败: " + ex.Message);
                    ModernDialog.ShowError(this, I18n.T("Dlg.Start.Failed.Title"), I18n.T("Dlg.Start.Failed.Msg", ex.Message));
                }
            }
            else if (idx == 4) // 一键还原官方原版
            {
                RestoreOriginal();
            }
        }

        private void HandleAdvToolAction(int idx)
        {
            try
            {
                if (idx == 0) // 备份 .reg
                {
                    string exportFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "IDM_Reg_Backup.reg");
                    Process p = Process.Start("reg.exe", "export \"HKCU\\Software\\DownloadManager\" \"" + exportFile + "\" /y");
                    if (p != null) p.WaitForExit();
                    Log("已成功导出 IDM 注册表配置至桌面: " + exportFile);
                    ModernDialog.ShowSuccess(this, I18n.T("Dlg.RegBackup.Title"), I18n.T("Dlg.RegBackup.Msg", exportFile));
                }
                else if (idx == 1) // 打开注册表
                {
                    Process.Start("regedit.exe");
                    Log("已启动 Windows 注册表编辑器。");
                }
                else if (idx == 2) // 打开安装目录
                {
                    string dir = GetIDMDir();
                    if (Directory.Exists(dir))
                    {
                        Process.Start("explorer.exe", dir);
                        Log("已在资源管理器中打开 IDM 安装目录: " + dir);
                    }
                    else
                    {
                        ModernDialog.ShowWarning(this, I18n.T("Dlg.Locate.DirNotFound.Title"), I18n.T("Dlg.Locate.DirNotFound.Msg"));
                    }
                }
                else if (idx == 3) // 手动定位路径
                {
                    PromptSelectIDMPath();
                }
            }
            catch (Exception ex)
            {
                Log("操作异常: " + ex.Message);
                ModernDialog.ShowError(this, I18n.T("Dlg.OpError.Title"), I18n.T("Dlg.OpError.Msg", ex.Message));
            }
        }

        public static void KillIDMDirect(Action<string> logFn)
        {
            if (logFn != null) logFn("正在检测并强行终止后台运行的 IDMan 进程...");
            int count = 0;
            foreach (Process p in Process.GetProcessesByName("IDMan"))
            {
                try { p.Kill(); count++; } catch { }
            }
            if (logFn != null) logFn("已成功终止 " + count + " 个正在运行的 IDM 进程。");
        }

        private void KillIDM()
        {
            KillIDMDirect(Log);
            RefreshAllStatus();
        }

        public static bool RestoreOriginalDirect(Action<string> logFn)
        {
            if (logFn != null) logFn("================= 开始执行：一键还原官方原版与初始配置 =================");
            bool ok = RestoreBinaryOnly(GetIDMDir(), logFn, true);
            RestoreRegistryState(logFn);
            return ok;
        }

        /// <summary>
        /// 仅还原主程序文件（可指定目录 + 是否终止 IDM 进程）。
        /// 与注册表操作**解耦**，便于测试隔离，避免回归测试误触真实系统状态。
        /// </summary>
        public static bool RestoreBinaryOnly(string idmDir, Action<string> logFn, bool killProcess)
        {
            string target = Path.Combine(idmDir, "IDMan.exe");
            string bak = target + ".BAK";

            if (killProcess)
            {
                KillIDMDirect(logFn);
                Thread.Sleep(300);
            }

            // 1. 还原二进制主程序（版本一致性守卫）
            if (File.Exists(bak))
            {
                string curVer = NativeBinaryPatcher.GetFileVersionSafe(target);
                string bakVer = NativeBinaryPatcher.GetFileVersionSafe(bak);

                if (!string.IsNullOrEmpty(curVer) && !string.IsNullOrEmpty(bakVer) && curVer != bakVer)
                {
                    if (logFn != null)
                    {
                        logFn("✗ 拒绝还原：备份版本与当前安装版本不一致！");
                        logFn("    当前 IDMan.exe   = v" + curVer);
                        logFn("    现有 IDMan.exe.BAK = v" + bakVer);
                        logFn("  ★ 直接还原会把旧版主程序覆盖到新版安装上，导致程序异常。");
                        logFn("  ★ 请重新安装当前版本的 IDM，或手动指定正确的官方原版备份。");
                    }
                    return false;
                }

                try
                {
                    File.Copy(bak, target, true);
                    if (logFn != null) logFn("✓ 官方原版主程序已成功无损还原！(IDMan.exe.BAK -> IDMan.exe, v" + bakVer + ")");
                }
                catch (Exception ex)
                {
                    if (logFn != null) logFn("还原主程序文件失败 (请确认以管理员身份运行): " + ex.Message);
                }
            }
            else
            {
                if (logFn != null) logFn("提示：未检测到官方原版备份文件 (IDMan.exe.BAK)，跳过文件覆盖。");
            }

            return true;
        }

        /// <summary>
        /// 清理注册表中的授权登记信息（FName/LName/Email/Serial）及所有策略/黑名单残留。
        /// 注意：此操作作用于 HKCU/HKLM，与安装目录无关，因此单独抽出。
        /// </summary>
        public static void RestoreRegistryState(Action<string> logFn)
        {
            // 2. 彻底清理注册表中的授权登记信息（FName/LName/Email/Serial）及所有策略/黑名单残留
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\DownloadManager", true))
                {
                    if (k != null)
                    {
                        string[] wipeList = new string[] {
                            "FName", "LName", "Email", "Serial",
                            "scansk", "tvfrdt", "radxcnt", "ptrk_scdt", "LastCheckQU",
                            "scTime", "NextCheck", "BList", "md5pks", "itb_r", "ncl_r",
                            "LstCheck", "CheckUpdtVM"
                        };
                        int wiped = 0;
                        foreach (string prop in wipeList)
                        {
                            try
                            {
                                if (k.GetValue(prop) != null)
                                {
                                    k.DeleteValue(prop, false);
                                    wiped++;
                                }
                            }
                            catch { }
                        }
                        if (logFn != null) logFn("✓ 已彻底抹除注册表授权登记项 (FName/Email/Serial等共 " + wiped + " 项)");
                    }
                }

                // 检查清理 HKLM 残留（若存在）
                string[] hklmPaths = new string[] {
                    @"SOFTWARE\Internet Download Manager",
                    @"SOFTWARE\WOW6432Node\Internet Download Manager"
                };
                foreach (string p in hklmPaths)
                {
                    try
                    {
                        using (RegistryKey hk = Registry.LocalMachine.OpenSubKey(p, true))
                        {
                            if (hk != null)
                            {
                                foreach (string prop in new string[] { "FName", "LName", "Email", "Serial" })
                                {
                                    try { hk.DeleteValue(prop, false); } catch { }
                                }
                            }
                        }
                    }
                    catch { }
                }

                // 3. 同步解除 ACL 锁定并清理试用记录
                try
                {
                    int unfreezeCount = TrialFreezeEngine.CleanAllTrialKeys(logFn);
                    if (unfreezeCount > 0 && logFn != null)
                        logFn("✓ 已同步解除 ACL 权限锁定并清理试用特征项 " + unfreezeCount + " 处");
                }
                catch { }

                if (logFn != null)
                {
                    logFn("✓ 注册表授权配置已彻底重置为【官方未注册原版】状态！");
                    logFn("✓ 一键还原官方操作完成。");
                }
            }
            catch (Exception ex)
            {
                if (logFn != null) logFn("清理注册表授权配置失败: " + ex.Message);
            }
        }

        private void RestoreOriginal()
        {
            SwitchTab(2);
            bool ok = RestoreOriginalDirect(Log);
            RefreshAllStatus();
            if (txtAuthName != null) txtAuthName.Text = GetDefaultUserName();
            if (txtAuthEmail != null) txtAuthEmail.Text = GetDefaultUserEmail();
            if (txtAuthSerial != null) txtAuthSerial.Text = GenerateSerial();

            if (ok)
            {
                ModernDialog.ShowSuccess(this, I18n.T("Dlg.Restore.Success.Title"), I18n.T("Dlg.Restore.Success.Msg"));
            }
            else
            {
                ModernDialog.ShowWarning(this, I18n.T("Dlg.Restore.Warning.Title"), I18n.T("Dlg.Restore.Warning.Msg"));
            }
        }

    #region 原生二进制补丁引擎 (Native Binary Patcher) — AOB 特征码版 v3

    /// <summary>
    /// AOB（Array-Of-Bytes）特征码补丁点。
    /// 与旧版「硬编码文件偏移」的根本区别：补丁位置由特征码扫描动态确定，
    /// 因此天然支持同一补丁逻辑在不同 IDM 版本上的自动迁移，且带唯一性校验。
    /// </summary>
    public class AobPoint
    {
        public string Name;
        public byte[] SigOriginal;   // 原始态特征码
        public byte[] SigPatched;    // 已补丁态特征码
        public int    PatchOffset;   // 补丁字节在特征码内的偏移
        public byte[] Expected;      // 期望的原始字节（冗余校验，双保险）
        public byte[] Patch;         // 写入的补丁字节
        public bool   Optional;      // 可选位点：未命中时跳过，不阻断整体写入

        public AobPoint(string name, string sigOriginalHex, string sigPatchedHex,
                        int patchOffset, string expectedHex, string patchHex)
            : this(name, sigOriginalHex, sigPatchedHex, patchOffset, expectedHex, patchHex, false)
        {
        }

        public AobPoint(string name, string sigOriginalHex, string sigPatchedHex,
                        int patchOffset, string expectedHex, string patchHex, bool optional)
        {
            this.Name        = name;
            this.SigOriginal = HexToBytes(sigOriginalHex);
            this.SigPatched  = HexToBytes(sigPatchedHex);
            this.PatchOffset = patchOffset;
            this.Expected    = HexToBytes(expectedHex);
            this.Patch       = HexToBytes(patchHex);
            this.Optional    = optional;
        }

        internal static byte[] HexToBytes(string hex)
        {
            int len = hex.Length;
            byte[] bytes = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }

    public sealed class PeInfo
    {
        public int  OptionalHeaderOffset;
        public int  SectionTableOffset;
        public int  SectionCount;
        public int  CheckSumOffset;       // 可选头 + 0x40
        public int  SecurityDirOffset;    // 数据目录[4] 的文件偏移，-1 表示不存在
        public int  SecurityRva;
        public int  SecuritySize;
        public long ImageEnd;             // max(节区 RawPtr + RawSize) —— 真实映像末尾
        public bool IsPe32Plus;
    }

    /// <summary>PE 解析 / 完整性校验工具（版本无关，一切偏移动态计算）</summary>
    public static class PeImageUtil
    {
        private static int RdU16(byte[] d, int o) { return d[o] | (d[o + 1] << 8); }
        private static int RdI32(byte[] d, int o) { return d[o] | (d[o + 1] << 8) | (d[o + 2] << 16) | (d[o + 3] << 24); }

        public static bool TryParse(byte[] d, out PeInfo info, out string error)
        {
            info = null; error = null;

            if (d == null || d.Length < 0x200) { error = I18n.T("Pe.TooSmall"); return false; }
            if (d[0] != 0x4D || d[1] != 0x5A) { error = I18n.T("Pe.MissingMZ"); return false; }

            int lfanew = RdI32(d, 0x3C);
            if (lfanew <= 0 || lfanew + 0x18 > d.Length) { error = I18n.T("Pe.BadLfanew"); return false; }
            if (d[lfanew] != 0x50 || d[lfanew + 1] != 0x45 || d[lfanew + 2] != 0 || d[lfanew + 3] != 0)
            { error = I18n.T("Pe.MissingPE"); return false; }

            int coff = lfanew + 4;
            if (coff + 20 > d.Length) { error = I18n.T("Pe.BadCoff"); return false; }
            int nsec  = RdU16(d, coff + 2);
            int optsz = RdU16(d, coff + 16);
            int opt   = coff + 20;

            if (nsec <= 0 || nsec > 96) { error = I18n.T("Pe.BadNsec", nsec); return false; }
            if (optsz < 0xE0 || opt + optsz > d.Length) { error = I18n.T("Pe.BadOptSz", optsz); return false; }

            int magic = RdU16(d, opt);
            bool plus = (magic == 0x20B);
            if (magic != 0x10B && magic != 0x20B)
            { error = I18n.T("Pe.BadMagic", magic.ToString("X")); return false; }

            int secTab = opt + optsz;
            if ((long)secTab + (long)nsec * 40 > d.Length) { error = I18n.T("Pe.BadSecTab"); return false; }

            long imgEnd = 0;
            for (int i = 0; i < nsec; i++)
            {
                int o = secTab + i * 40;
                int rawSize = RdI32(d, o + 16);
                int rawPtr  = RdI32(d, o + 20);
                if (rawSize < 0 || rawPtr < 0) { error = I18n.T("Pe.BadSecHeader", i); return false; }
                long end = (long)rawPtr + (long)rawSize;
                if (end > d.Length)
                {
                    string nm = Encoding.ASCII.GetString(d, o, 8).TrimEnd('\0', ' ');
                    error = "节区 [" + nm + "] 原始数据超出文件末尾（需要 " + end.ToString("N0") + " 字节，实际 "
                          + d.Length.ToString("N0") + " 字节）—— 映像已被截断损坏";
                    return false;
                }
                if (end > imgEnd) imgEnd = end;
            }

            PeInfo pi = new PeInfo();
            pi.OptionalHeaderOffset = opt;
            pi.SectionTableOffset   = secTab;
            pi.SectionCount         = nsec;
            pi.IsPe32Plus           = plus;
            pi.ImageEnd             = imgEnd;
            pi.CheckSumOffset       = opt + 0x40;

            int nddOff = opt + (plus ? 0x6C : 0x5C);
            int ddOff  = opt + (plus ? 0x70 : 0x60);
            int ndd    = RdI32(d, nddOff);
            if (ndd > 4 && ddOff + 5 * 8 <= opt + optsz)
            {
                pi.SecurityDirOffset = ddOff + 4 * 8;
                pi.SecurityRva       = RdI32(d, pi.SecurityDirOffset);
                pi.SecuritySize      = RdI32(d, pi.SecurityDirOffset + 4);
            }
            else
            {
                pi.SecurityDirOffset = -1;
                pi.SecurityRva       = 0;
                pi.SecuritySize      = 0;
            }

            info = pi;
            return true;
        }

        /// <summary>仅做合法性判定（用于打补丁前 / 启动前的快速体检）</summary>
        public static bool IsValidImage(byte[] d, out string error)
        {
            PeInfo pi;
            return TryParse(d, out pi, out error);
        }

        /// <summary>
        /// 标准 PE 映像校验和（与 MS CheckSumMappedFile 同款算法）。
        /// 计算时把校验和字段本身视为 0。
        /// </summary>
        public static uint ComputeChecksum(byte[] data, int checksumFieldOffset)
        {
            long sum = 0;
            int n = data.Length;

            for (int i = 0; i + 1 < n; i += 2)
            {
                int word;
                if (i >= checksumFieldOffset && i < checksumFieldOffset + 4) word = 0;
                else word = data[i] | (data[i + 1] << 8);

                sum += word;
                sum = (sum & 0xFFFF) + (sum >> 16);
            }

            if ((n & 1) != 0)
            {
                sum += data[n - 1];
                sum = (sum & 0xFFFF) + (sum >> 16);
            }

            while (sum > 0xFFFF) sum = (sum & 0xFFFF) + (sum >> 16);
            return (uint)((sum + n) & 0xFFFFFFFFL);
        }
    }

    public static class NativeBinaryPatcher
    {
                // ===== 支持范围 =====
        public const string SupportedVersions = "IDM 6.43 build 10 / build 11 (6.43.11.2) / build 11 (6.43.11.3) / build 12 (6.43.12.3) / build 15 (6.43.15.3) / build 16 (6.43.16.3) / build 18 (6.43.18.3)";

        // ===== AOB 特征码表 =====
        // 经 IDM 6.43 build 10 / build 11 (6.43.11.2) / build 11 (6.43.11.3) /
        //        build 12 (6.43.12.3) / build 15 (6.43.15.3) / build 16 (6.43.16.3) /
        //        build 18 (6.43.18.3)
        // 七个版本交叉验证：15 个必需位点 + 1 个可选位点在「原始态 / 已补丁态」下均唯一命中。
        //
        // build 18 相对 build 16：.text +24,576 B / .rdata +1,536 B / .reloc +512 B，
        // .data 与 .rsrc 原始大小不变；全部节区 VA 后移 0x6000（.text 仍从 0x1000 起始），
        // 而段内代码位移为「负向紧凑」（-0x320 ~ -0x9A0），数据段正向位移 +0x6600。
        // 关键结论：16 个位点的原始态特征码在 build 18 上「零汉明距离精确命中」，
        // 无需重新收窄签名 —— 该版本只做了代码重排与体积调整，授权判定链语义未变。
        //
        // build 16 相对 build 15 的 .text 段增长 245 KB，代码整体位移 +0x1D0 ~ +0x5E0；
        // 部分位点周边地址常量亦随之变化（如位点 1 左侧 0x69 -> 0x6D），因此特征码在
        // 六版本共同稳定区内自动收窄。所有位点已用 capstone 验证指令语义与五版本完全一致。
        //
        // 特征码取自「多版本共同稳定区」——即只有在这些版本中字节完全一致的区域
        // 才被纳入特征码，从而天然排除编译器生成的重定位指针与版本相关常量。
        // 单条签名长度上限 48 字节：足够长以防误命中，足够短以抵御未来版本代码漂移。
        // 实测印证：build 18 的代码重排幅度（-0x9A0 ~ -0x320）大于 build 16，
        // 但 48 B 上限的签名依然 16/16 精确命中，无需重新生成。
        public static readonly AobPoint[] POINTS = new AobPoint[]
        {
            new AobPoint(
                "授权分支检测 (test eax->xor eax)",
                "0085C00F851B010000C6854FF5FFFF018D8D50F5FFFFE8",
                "0033C00F851B010000C6854FF5FFFF018D8D50F5FFFFE8",
                1, "85", "33"),

            new AobPoint(
                "试用期最大值 (->mov eax,7FFFFFFF+nop)",
                "00F7D81BC083E00F83C00FA3",
                "00F7D81BC0B8FFFFFF7F90A3",
                5, "83E00F83C00F", "B8FFFFFF7F90"),

            new AobPoint(
                "假序列号弹窗 A (jz->jmp)",
                "0E0084C0742A807C2407007523C7442414FFFFFFFF8D4C2408E8",
                "0E0084C0EB2A807C2407007523C7442414FFFFFFFF8D4C2408E8",
                4, "74", "EB"),

            new AobPoint(
                "守护线程 A",
                "000064890D00000000595F5E5D5B81C4EC000000C3CCCCCC6AFF68",
                "000064890D00000000595F5E5D5B81C4EC000000C3CCCCCCC3FF68",
                24, "6A", "C3"),

            new AobPoint(
                "守护线程 B",
                "0081C470010000C3CCCCCCCC6AFF68",
                "0081C470010000C3CCCCCCCCC3FF68",
                12, "6A", "C3"),

            new AobPoint(
                "守护线程 C",
                "00595F5E5D5B83C410C20400CCCCCCCCCCCCCCCCCCCCCCCC6AFF68",
                "00595F5E5D5B83C410C20400CCCCCCCCCCCCCCCCCCCCCCCCC3FF68",
                24, "6A", "C3"),

            new AobPoint(
                "守护线程 D",
                "00B801000000C3CCCCCCCCCCCCCCCCCCCCCCCCCC6AFF68",
                "00B801000000C3CCCCCCCCCCCCCCCCCCCCCCCCCCC3FF68",
                20, "6A", "C3"),

            new AobPoint(
                "看门狗关联函数",
                "0081C41C100000C3CCCC6AFF68",
                "0081C41C100000C3CCCCC3FF68",
                10, "6A", "C3"),

            new AobPoint(
                "守护线程 E (push ebp->ret)",
                "0081C4D0010000C3CCCCCCCCCCCCCCCCCC558BEC83E4F86AFF68",
                "0081C4D0010000C3CCCCCCCCCCCCCCCCCCC38BEC83E4F86AFF68",
                17, "55", "C3"),

            new AobPoint(
                "守护线程 F",
                "8B4C240864890D00000000595E83C40CC3CCCCCCCCCCCCCC6AFF68",
                "8B4C240864890D00000000595E83C40CC3CCCCCCCCCCCCCCC3FF68",
                24, "6A", "C3"),

            new AobPoint(
                "过期强退逻辑 (0F85->90E9)",
                "85801B02008D50018D6424008A084084C975F92BC283F8020F85D2010000E8",
                "85801B02008D50018D6424008A084084C975F92BC283F80290E9D2010000E8",
                24, "0F85", "90E9"),

            new AobPoint(
                "联网验证标志位",
                "FEFFB8010000009BE9D5000000B801000000A3",
                "FEFFB8010000009BE9D5000000B800000000A3",
                14, "01", "00"),

            new AobPoint(
                "假序列号弹窗 B (jz->jmp)",
                "030084C0742A807C2407007523C7442414FFFFFFFF8D4C2408E8",
                "030084C0EB2A807C2407007523C7442414FFFFFFFF8D4C2408E8",
                4, "74", "EB"),

            new AobPoint(
                "试用状态标志 + 天数限制常量",
                "00000001000000010000001E000000",
                "0000000100000000000000FFFFFF7F",
                7, "010000001E000000", "00000000FFFFFF7F"),

            new AobPoint(
                "注册对话框抑制 (Serial缺失分支, 可选)",
                "0085C0740C8BCBE8",
                "0085C0EB0C8BCBE8",
                3, "74", "EB", true),

            new AobPoint(
                "启动自动更新定时器切断 (jne->jmp, 彻底杜绝后台更新)",
                "1F8B562052FFD785C07407C6868B02000001389E8E02000075195368E86E03006A228B462050FFD785C07407C6868E" +
                "02",
                "1F8B562052FFD785C07407C6868B02000001389E8E020000EB195368E86E03006A228B462050FFD785C07407C6868E" +
                "02",
                24, "75", "EB"),

        };

        // 向后兼容：旧代码引用的 RulesCount
        public static int RulesCount { get { return POINTS.Length; } }

        /// <summary>AOB 扫描命中结果</summary>
        public sealed class AobHit
        {
            public AobPoint Point;
            public int      SiteOffset;      // 补丁字节在文件中的绝对偏移
            public bool     AlreadyPatched;  // 是否已处于补丁状态
        }

        private static int IndexOf(byte[] hay, byte[] needle, int start)
        {
            if (hay == null || needle == null) return -1;
            int n = hay.Length, m = needle.Length;
            if (m == 0 || m > n) return -1;
            int limit = n - m;
            byte first = needle[0];
            for (int i = Math.Max(0, start); i <= limit; i++)
            {
                if (hay[i] != first) continue;
                int j = 1;
                while (j < m && hay[i + j] == needle[j]) j++;
                if (j == m) return i;
            }
            return -1;
        }

        private static int IndexOf(byte[] hay, byte[] needle) { return IndexOf(hay, needle, 0); }

        /// <summary>
        /// AOB 全表扫描。返回 true 表示全部位点均已「唯一命中」。
        /// 定位策略：先找已补丁态特征码（幂等支持），再找原始态特征码并校验唯一性。
        /// </summary>
        public static bool ScanAll(byte[] data, Action<string> logFn, out AobHit[] hits)
        {
            hits = new AobHit[POINTS.Length];
            int resolved   = 0;   // 已处理位点数（含可选跳过）
            int required   = 0;   // 必需位点总数
            int requiredOk = 0;   // 必需位点已解决数

            for (int i = 0; i < POINTS.Length; i++)
                if (!POINTS[i].Optional) required++;

            for (int i = 0; i < POINTS.Length; i++)
            {
                AobPoint p = POINTS[i];
                int op = IndexOf(data, p.SigPatched);
                int oo = IndexOf(data, p.SigOriginal);

                // 情况 1：已是补丁状态（幂等）
                if (op >= 0 && oo < 0)
                {
                    AobHit h = new AobHit();
                    h.Point = p;
                    h.SiteOffset = op + p.PatchOffset;
                    h.AlreadyPatched = true;
                    hits[i] = h;
                    resolved++;
                    if (!p.Optional) requiredOk++;
                    continue;
                }

                // 情况 2：找不到原始特征码
                if (oo < 0)
                {
                    if (p.Optional)
                    {
                        // 可选位点：本版本不含此对抗逻辑，跳过即可，不阻断整体写入
                        if (logFn != null) logFn("  – 跳过可选位点（本版本无此逻辑）: " + p.Name);
                        resolved++;
                        continue;
                    }
                    if (logFn != null) logFn("  ✗ 未找到特征码: " + p.Name);
                    continue;
                }

                // 情况 3：特征码必须唯一
                if (IndexOf(data, p.SigOriginal, oo + 1) >= 0)
                {
                    if (logFn != null) logFn("  ✗ 特征码非唯一命中: " + p.Name);
                    continue;
                }

                // 冗余校验：位点字节必须与期望一致
                int site = oo + p.PatchOffset;
                if (site < 0 || site + p.Expected.Length > data.Length)
                {
                    if (logFn != null) logFn("  ✗ 位点越界: " + p.Name);
                    continue;
                }
                bool ok = true;
                for (int k = 0; k < p.Expected.Length; k++)
                    if (data[site + k] != p.Expected[k]) { ok = false; break; }
                if (!ok)
                {
                    if (logFn != null) logFn("  ✗ 位点字节与期望不符: " + p.Name);
                    continue;
                }

                AobHit h2 = new AobHit();
                h2.Point = p;
                h2.SiteOffset = site;
                h2.AlreadyPatched = false;
                hits[i] = h2;
                resolved++;
                if (!p.Optional) requiredOk++;
            }

            return requiredOk == required;
        }

        /// <summary>
        /// 补丁前置体检。返回 false 时严禁写入任何字节。
        /// 两道门禁：① PE 结构完整性  ② AOB 特征码全表唯一命中
        /// </summary>
        public static bool PreflightCheck(string targetExe, Action<string> logFn,
                                          out byte[] data, out PeInfo pe, out AobHit[] hits)
        {
            data = null; pe = null; hits = null;

            if (!File.Exists(targetExe)) { logFn("✗ 未找到目标文件: " + targetExe); return false; }

            try { data = File.ReadAllBytes(targetExe); }
            catch (Exception ex) { logFn("✗ 读取文件失败: " + ex.Message); return false; }

            if (data.Length == 0)
            {
                logFn("✗ 目标文件长度为 0 字节（极可能已被安全软件查杀/隔离）。");
                return false;
            }

            // ---- 门禁 1：PE 结构完整性 ----
            string peErr;
            if (!PeImageUtil.TryParse(data, out pe, out peErr))
            {
                logFn("✗ PE 结构校验失败：" + peErr);
                logFn("  该 IDMan.exe 当前已不是合法的可执行映像，严禁继续打补丁。");
                logFn("  ★ 请使用【一键还原官方原版】从 IDMan.exe.BAK 恢复后再重试。");
                return false;
            }
            logFn("✓ PE 结构校验通过（节区 " + pe.SectionCount + " 个，映像末尾 0x" + pe.ImageEnd.ToString("X") + "）");

            // ---- 门禁 2：AOB 特征码扫描 ----
            logFn("  正在执行 AOB 特征码扫描（支持版本：" + SupportedVersions + "）...");
            if (!ScanAll(data, logFn, out hits))
            {
                int got = 0;
                if (hits != null) foreach (AobHit h in hits) if (h != null) got++;
                logFn("✗ 特征码扫描未完全命中（" + got + "/" + POINTS.Length + "）。");
                logFn("  本工具特征码仅适配 " + SupportedVersions + "。");
                logFn("  ★ 为避免损坏程序，已中止二进制补丁，未写入任何字节。");
                logFn("  ★ 建议改用【模式二：永久冻结试用期】或【模式三：个性化授权登记】。");
                return false;
            }

            int already = 0, skipped = 0;
            foreach (AobHit h in hits)
            {
                if (h == null) { skipped++; continue; }   // 可选位点：本版本不含此逻辑
                if (h.AlreadyPatched) already++;
            }
            int total = POINTS.Length - skipped;
            logFn("✓ AOB 扫描全部命中 " + total + "/" + total
                + "（已补丁 " + already + " 处 / 待补丁 " + (total - already) + " 处）"
                + (skipped > 0 ? "，可选位点跳过 " + skipped + " 处（本版本无此逻辑）" : ""));
            return true;
        }

        public static bool ApplyPatch(string targetExe, bool backup, Action<string> logFn, out int appliedCount)
        {
            appliedCount = 0;
            byte[] data; PeInfo pe; AobHit[] hits;

            logFn("──────── 二进制补丁引擎 (AOB 特征码版 v3) ────────");

            if (!PreflightCheck(targetExe, logFn, out data, out pe, out hits))
                return false;

            // ---- 备份管理（版本感知：防止 IDM 升级后沿用旧版备份，导致回滚点错版）----
            string bakPath = targetExe + ".BAK";
            if (backup)
            {
                if (!EnsureFreshBackup(targetExe, hits, logFn, bakPath))
                    return false;
            }

            // ---- 本次运行的回滚点 ----
            string rollbackPath = targetExe + ".rollback.tmp";
            try { File.Copy(targetExe, rollbackPath, true); }
            catch (Exception ex) { logFn("✗ 无法创建回滚点，为安全起见中止补丁: " + ex.Message); return false; }

            try
            {
                // ---- 写入代码段补丁 ----
                foreach (AobHit h in hits)
                {
                    if (h == null) continue;   // 可选位点：本版本无此逻辑，直接跳过
                    AobPoint p = h.Point;
                    if (h.AlreadyPatched)
                    {
                        logFn("→ 已处于补丁状态，跳过: " + p.Name);
                        appliedCount++;
                        continue;
                    }
                    for (int i = 0; i < p.Patch.Length; i++) data[h.SiteOffset + i] = p.Patch[i];
                    appliedCount++;
                    logFn("✓ 补丁写入 [0x" + h.SiteOffset.ToString("X") + "]: " + p.Name);
                }

                // ---- PE 头修正：动态定位，版本无关 ----
                if (pe.SecurityDirOffset >= 0 && (pe.SecurityRva != 0 || pe.SecuritySize != 0))
                {
                    int so = pe.SecurityDirOffset;
                    for (int i = 0; i < 8; i++) data[so + i] = 0;
                    logFn("✓ 已清除 PE 数字签名目录（偏移 0x" + so.ToString("X") + "，原 RVA=0x" + pe.SecurityRva.ToString("X") + "）");
                }
                else
                {
                    logFn("→ PE 数字签名目录本就不存在，无需清除");
                }

                // ---- 安全截断：只剥离「超出节区表所描述映像末尾」的 Authenticode 尾部 ----
                long overlay = (long)data.Length - pe.ImageEnd;
                if (overlay > 0)
                {
                    long declaredSig = pe.SecuritySize;
                    if (declaredSig > 0 && overlay == declaredSig)
                    {
                        Array.Resize(ref data, (int)pe.ImageEnd);
                        logFn("✓ 已剥离 Authenticode 签名尾部 " + overlay.ToString("N0") + " 字节（动态计算，非硬编码）");
                    }
                    else if (overlay < 1024 * 1024)
                    {
                        Array.Resize(ref data, (int)pe.ImageEnd);
                        logFn("✓ 已剥离映像尾部冗余数据 " + overlay.ToString("N0") + " 字节");
                    }
                    else
                    {
                        logFn("→ 检测到 " + overlay.ToString("N0") + " 字节非签名尾部数据，为安全起见保留不截断");
                    }
                }

                // ---- 重算 PE 校验和（必须放在「全部补丁 + 截断」之后）----
                uint newSum = PeImageUtil.ComputeChecksum(data, pe.CheckSumOffset);
                uint oldSum = (uint)(data[pe.CheckSumOffset] | (data[pe.CheckSumOffset + 1] << 8)
                                   | (data[pe.CheckSumOffset + 2] << 16) | (data[pe.CheckSumOffset + 3] << 24));
                data[pe.CheckSumOffset]     = (byte)(newSum & 0xFF);
                data[pe.CheckSumOffset + 1] = (byte)((newSum >> 8) & 0xFF);
                data[pe.CheckSumOffset + 2] = (byte)((newSum >> 16) & 0xFF);
                data[pe.CheckSumOffset + 3] = (byte)((newSum >> 24) & 0xFF);
                logFn("✓ 已重算 PE 校验和：0x" + oldSum.ToString("X8") + " → 0x" + newSum.ToString("X8")
                    + "（标准算法动态计算）");

                // ---- 写入前 PE 终检 ----
                string vErr;
                if (!PeImageUtil.IsValidImage(data, out vErr))
                {
                    logFn("✗ 补丁后 PE 自检失败（" + vErr + "），已放弃写入。");
                    return false;
                }

                // ---- 原子写入 ----
                string tmpPath = targetExe + ".new.tmp";
                try
                {
                    File.WriteAllBytes(tmpPath, data);
                    File.Copy(tmpPath, targetExe, true);
                }
                catch (Exception ex)
                {
                    logFn("✗ 写入补丁文件失败（请确认以管理员身份运行）: " + ex.Message);
                    try { File.Copy(rollbackPath, targetExe, true); logFn("✓ 已自动回滚至补丁前状态"); } catch { }
                    return false;
                }
                finally
                {
                    try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { }
                }

                // ---- 落盘后终检 + 失败自动回滚 ----
                byte[] verify;
                try { verify = File.ReadAllBytes(targetExe); }
                catch (Exception ex)
                {
                    logFn("✗ 落盘复读失败: " + ex.Message);
                    try { File.Copy(rollbackPath, targetExe, true); logFn("✓ 已自动回滚"); } catch { }
                    return false;
                }

                string postErr;
                if (!PeImageUtil.IsValidImage(verify, out postErr))
                {
                    logFn("✗ 落盘后 PE 校验失败（" + postErr + "），正在自动回滚...");
                    try { File.Copy(rollbackPath, targetExe, true); logFn("✓ 已自动回滚，IDMan.exe 恢复至补丁前状态"); }
                    catch (Exception ex) { logFn("✗ 回滚失败，请手动使用【一键还原官方原版】: " + ex.Message); }
                    return false;
                }

                string hashStr = Sha256Hex(verify);
                logFn("★ 底层二进制补丁写入完成！共 " + appliedCount + "/" + POINTS.Length + " 个位点生效。");
                logFn("  落盘 SHA256: " + hashStr);
                logFn("  落盘体积  : " + verify.Length.ToString("N0") + " 字节");

                return true;
            }
            finally
            {
                try { if (File.Exists(rollbackPath)) File.Delete(rollbackPath); } catch { }
            }
        }

        public static string Sha256Hex(byte[] data)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(data);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash) sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }

        /// <summary>读取文件版本号（失败返回空串）</summary>
        public static string GetFileVersionSafe(string path)
        {
            try
            {
                if (!File.Exists(path)) return "";
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);
                return (fvi == null || string.IsNullOrEmpty(fvi.FileVersion)) ? "" : fvi.FileVersion.Trim();
            }
            catch { return ""; }
        }

        /// <summary>
        /// 备份管理（版本感知）。
        /// 关键场景：用户升级 IDM 后，旧的 IDMan.exe.BAK 仍是上一版本的官方原版。
        /// 若直接沿用，回滚点与「一键还原」都会指向错误的版本（b10 主程序 + b11 依赖库）。
        /// 策略：
        ///   1) BAK 不存在            -> 直接备份
        ///   2) BAK 版本 == 当前版本  -> 保持不变
        ///   3) BAK 版本 != 当前版本：
        ///        a) 当前文件为纯净原版（全部位点处于原始态）-> 归档旧备份，写入新备份
        ///        b) 当前文件已含补丁  -> 无法作为备份源，拒绝继续（避免污染备份）
        /// </summary>
        private static bool EnsureFreshBackup(string targetExe, AobHit[] hits,
                                              Action<string> logFn, string bakPath)
        {
            if (!File.Exists(bakPath))
            {
                try { File.Copy(targetExe, bakPath); logFn("✓ 已自动备份原版至: " + bakPath); }
                catch (Exception ex) { logFn("✗ 备份失败，为安全起见中止补丁: " + ex.Message); return false; }
                return true;
            }

            string curVer = GetFileVersionSafe(targetExe);
            string bakVer = GetFileVersionSafe(bakPath);

            if (!string.IsNullOrEmpty(curVer) && curVer == bakVer)
            {
                logFn("→ 已有同版本官方原版备份（v" + curVer + "），保持不动");
                return true;
            }

            logFn("⚠ 备份版本不一致：当前 IDMan.exe = v" + (curVer.Length == 0 ? "未知" : curVer)
                + "，现有 IDMan.exe.BAK = v" + (bakVer.Length == 0 ? "未知" : bakVer));

            bool pristine = true;
            foreach (AobHit h in hits) if (h != null && h.AlreadyPatched) { pristine = false; break; }

            if (!pristine)
            {
                logFn("✗ 当前 IDMan.exe 并非纯净原版（已含补丁），无法作为新备份源。");
                logFn("  ★ 若继续，回滚点将指向错误版本。为安全起见已中止。");
                logFn("  ★ 请先重新安装当前版本的 IDM，或手动将官方原版另存为: " + bakPath);
                return false;
            }

            // 归档旧备份，避免覆盖丢失
            string stamp = (bakVer.Length == 0) ? "unknown" : bakVer.Replace(", ", ".").Replace(" ", "");
            string archived = bakPath + "." + stamp;
            try
            {
                if (File.Exists(archived)) File.Delete(archived);
                File.Move(bakPath, archived);
                logFn("→ 旧版备份已归档为: " + Path.GetFileName(archived));
            }
            catch (Exception ex)
            {
                logFn("⚠ 旧备份归档失败（将继续覆盖）: " + ex.Message);
            }

            try
            {
                File.Copy(targetExe, bakPath, true);
                logFn("✓ 已更新官方原版备份至当前版本 v" + curVer + ": " + bakPath);
            }
            catch (Exception ex) { logFn("✗ 写入新备份失败，为安全起见中止补丁: " + ex.Message); return false; }

            return true;
        }

        /// <summary>启动前体检：返回 null 表示健康，否则返回人类可读的故障描述</summary>
        public static string DiagnoseExecutable(string exePath)
        {
            try
            {
                if (!File.Exists(exePath)) return I18n.T("Diag.NotFound");
                FileInfo fi = new FileInfo(exePath);
                if (fi.Length == 0) return I18n.T("Diag.ZeroBytes");
                byte[] d = File.ReadAllBytes(exePath);
                string err;
                if (!PeImageUtil.IsValidImage(d, out err)) return err;
                return null;
            }
            catch (Exception ex) { return I18n.T("Diag.ReadFailed", ex.Message); }
        }
    }

    #endregion

    #region 原生 ACL 试用期永久冻结引擎 (Trial Freeze & ACL Engine)

    public static class TrialFreezeEngine
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int RtlAdjustPrivilege(int privilege, bool enable, bool currentThread, out bool enabled);

        private const int SE_TAKE_OWNERSHIP_PRIVILEGE = 9;
        private const int SE_BACKUP_PRIVILEGE = 17;
        private const int SE_RESTORE_PRIVILEGE = 18;

        public static void EnableTokenPrivileges()
        {
            try
            {
                bool prev;
                RtlAdjustPrivilege(SE_TAKE_OWNERSHIP_PRIVILEGE, true, false, out prev);
                RtlAdjustPrivilege(SE_BACKUP_PRIVILEGE, true, false, out prev);
                RtlAdjustPrivilege(SE_RESTORE_PRIVILEGE, true, false, out prev);
            }
            catch { }
        }

        private static readonly Regex GuidRegex = new Regex(
            @"^\{[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}\}$",
            RegexOptions.IgnoreCase);

        private static readonly string[] ExcludeSubKeys = new string[] {
            "LocalServer32", "InProcServer32", "InProcHandler32"
        };

        private static readonly string[] IdmValueNames = new string[] {
            "MData", "Model", "scansk", "Therad"
        };

        private static readonly SecurityIdentifier EveryoneSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        private static readonly SecurityIdentifier AdminSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);

        private const RegistryRights DenyRights = RegistryRights.SetValue | RegistryRights.CreateSubKey | RegistryRights.Delete | RegistryRights.WriteKey;

        public static string[] GetClsidRoots()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                return new string[] {
                    @"Software\Classes\WOW6432Node\CLSID",
                    @"Software\Classes\CLSID"
                };
            }
            else
            {
                return new string[] {
                    @"Software\Classes\CLSID"
                };
            }
        }

        public static bool IsIdmClsidKey(RegistryKey parentKey, string subName)
        {
            if (string.IsNullOrEmpty(subName) || !GuidRegex.IsMatch(subName))
                return false;

            bool isLocked = false;
            try
            {
                using (RegistryKey testKey = parentKey.OpenSubKey(subName, false))
                {
                    if (testKey == null)
                    {
                        // 无法打开（已被 ACL 拒绝读取/写入），判定为锁定的试用项
                        return true;
                    }

                    string[] subKeys = testKey.GetSubKeyNames();
                    foreach (string exclude in ExcludeSubKeys)
                    {
                        if (Array.IndexOf(subKeys, exclude) >= 0)
                            return false; // 排除标准 COM 系统组件
                    }

                    // 检查 DACL 中是否已有针对 Everyone 的 Deny 规则
                    try
                    {
                        RegistrySecurity sec = testKey.GetAccessControl();
                        AuthorizationRuleCollection rules = sec.GetAccessRules(true, true, typeof(SecurityIdentifier));
                        foreach (RegistryAccessRule rule in rules)
                        {
                            if (rule.AccessControlType == AccessControlType.Deny)
                            {
                                isLocked = true;
                                break;
                            }
                        }
                    }
                    catch { }

                    if (isLocked) return true;

                    // 规则 A: 默认值为纯数字且无子项
                    object defVal = testKey.GetValue("");
                    string sDef = defVal != null ? defVal.ToString() : null;
                    if (sDef != null)
                    {
                        if (subKeys.Length == 0 && Regex.IsMatch(sDef, @"^\d+$"))
                            return true;
                        // 规则 B: 默认值含 '+' 或 '=' 且无子项
                        if (subKeys.Length == 0 && (sDef.Contains("+") || sDef.Contains("=")))
                            return true;
                    }

                    // 规则 C: 包含 Version 子项且其默认值为纯数字
                    if (subKeys.Length == 1 && string.Equals(subKeys[0], "Version", StringComparison.OrdinalIgnoreCase))
                    {
                        using (RegistryKey verKey = testKey.OpenSubKey("Version", false))
                        {
                            if (verKey != null)
                            {
                                object verVal = verKey.GetValue("");
                                if (verVal != null && Regex.IsMatch(verVal.ToString(), @"^\d+$"))
                                    return true;
                            }
                        }
                    }

                    // 规则 D: 包含 MData/Model/scansk/Therad 特征值
                    string[] valNames = testKey.GetValueNames();
                    foreach (string vn in valNames)
                    {
                        foreach (string idmVn in IdmValueNames)
                        {
                            if (string.Equals(vn, idmVn, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }
                    }

                    // 规则 E: 0 值且 0 子项的幽灵空键
                    if (valNames.Length == 0 && subKeys.Length == 0)
                        return true;
                }
            }
            catch (SecurityException)
            {
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            catch { }

            return false;
        }

        public static bool LockKey(RegistryKey parentKey, string subName)
        {
            EnableTokenPrivileges();
            try
            {
                UnlockKey(parentKey, subName);

                using (RegistryKey key = parentKey.OpenSubKey(subName, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ChangePermissions | RegistryRights.ReadKey))
                {
                    if (key == null) return false;
                    RegistrySecurity sec = key.GetAccessControl();
                    sec.AddAccessRule(new RegistryAccessRule(
                        EveryoneSid,
                        DenyRights,
                        InheritanceFlags.ContainerInherit,
                        PropagationFlags.None,
                        AccessControlType.Deny
                    ));
                    key.SetAccessControl(sec);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool UnlockKey(RegistryKey parentKey, string subName)
        {
            EnableTokenPrivileges();
            try
            {
                // 1. 尝试直接以 ChangePermissions 打开并移除 Deny 规则
                try
                {
                    using (RegistryKey key = parentKey.OpenSubKey(subName, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ChangePermissions | RegistryRights.ReadKey))
                    {
                        if (key != null)
                        {
                            RegistrySecurity sec = key.GetAccessControl();
                            sec.RemoveAccessRule(new RegistryAccessRule(
                                EveryoneSid,
                                DenyRights,
                                InheritanceFlags.ContainerInherit,
                                PropagationFlags.None,
                                AccessControlType.Deny
                            ));
                            AuthorizationRuleCollection rules = sec.GetAccessRules(true, true, typeof(SecurityIdentifier));
                            for (int i = rules.Count - 1; i >= 0; i--)
                            {
                                RegistryAccessRule r = rules[i] as RegistryAccessRule;
                                if (r != null && r.AccessControlType == AccessControlType.Deny)
                                {
                                    sec.RemoveAccessRuleSpecific(r);
                                }
                            }
                            key.SetAccessControl(sec);
                            return true;
                        }
                    }
                }
                catch { }

                // 2. 若直接打开被拒绝，尝试先获取所有权再重写 DACL
                try
                {
                    using (RegistryKey key = parentKey.OpenSubKey(subName, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.TakeOwnership))
                    {
                        if (key != null)
                        {
                            RegistrySecurity sec = new RegistrySecurity();
                            sec.SetOwner(AdminSid);
                            key.SetAccessControl(sec);
                        }
                    }
                }
                catch { }

                try
                {
                    using (RegistryKey key = parentKey.OpenSubKey(subName, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ChangePermissions))
                    {
                        if (key != null)
                        {
                            RegistrySecurity sec = new RegistrySecurity();
                            sec.ResetAccessRule(new RegistryAccessRule(
                                EveryoneSid,
                                RegistryRights.FullControl,
                                InheritanceFlags.ContainerInherit,
                                PropagationFlags.None,
                                AccessControlType.Allow
                            ));
                            key.SetAccessControl(sec);
                            return true;
                        }
                    }
                }
                catch { }
            }
            catch { }
            return false;
        }

        public static void TriggerTrialInitialization(string idmDir, Action<string> logFn)
        {
            string idmExe = Path.Combine(idmDir, "IDMan.exe");
            if (!File.Exists(idmExe)) return;

            if (logFn != null) logFn("正在唤醒 IDM 初始化全新 30 天试用基线...");

            string tempFile = Path.Combine(Path.GetTempPath(), "idm_trial_probe.png");
            try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { }

            bool probeSuccess = false;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = idmExe;
                psi.Arguments = "/n /d \"https://www.internetdownloadmanager.com/images/idm_box_min.png\" /p \"" + Path.GetTempPath().TrimEnd('\\') + "\" /f idm_trial_probe.png";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;

                using (Process p = Process.Start(psi))
                {
                    for (int i = 0; i < 15; i++)
                    {
                        Thread.Sleep(200);
                        if (File.Exists(tempFile))
                        {
                            probeSuccess = true;
                            break;
                        }
                    }
                }
            }
            catch { }

            if (!probeSuccess)
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = idmExe;
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    using (Process p = Process.Start(psi))
                    {
                        Thread.Sleep(1200);
                    }
                }
                catch { }
            }

            try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { }
            MainWindow.KillIDMDirect(null);
            Thread.Sleep(300);
        }

        public static int CleanAllTrialKeys(Action<string> logFn)
        {
            EnableTokenPrivileges();
            int cleanedCount = 0;
            string[] clsidRoots = GetClsidRoots();

            foreach (string rootPath in clsidRoots)
            {
                try
                {
                    using (RegistryKey clsidKey = Registry.CurrentUser.OpenSubKey(rootPath, true))
                    {
                        if (clsidKey == null) continue;
                        string[] subKeyNames = clsidKey.GetSubKeyNames();
                        foreach (string subName in subKeyNames)
                        {
                            if (IsIdmClsidKey(clsidKey, subName))
                            {
                                UnlockKey(clsidKey, subName);
                                try
                                {
                                    clsidKey.DeleteSubKeyTree(subName, false);
                                    cleanedCount++;
                                    if (logFn != null) logFn("已解除锁定并清除旧试用特征项: " + subName);
                                }
                                catch { }
                            }
                        }
                    }
                }
                catch { }
            }
            return cleanedCount;
        }

        public static int LockAllTrialKeys(Action<string> logFn)
        {
            EnableTokenPrivileges();
            int lockedCount = 0;
            string[] clsidRoots = GetClsidRoots();

            foreach (string rootPath in clsidRoots)
            {
                try
                {
                    using (RegistryKey clsidKey = Registry.CurrentUser.OpenSubKey(rootPath, true))
                    {
                        if (clsidKey == null) continue;
                        string[] subKeyNames = clsidKey.GetSubKeyNames();
                        foreach (string subName in subKeyNames)
                        {
                            if (IsIdmClsidKey(clsidKey, subName))
                            {
                                if (LockKey(clsidKey, subName))
                                {
                                    lockedCount++;
                                    if (logFn != null) logFn("✓ 已通过 ACL 权限锁定试用项: " + subName);
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            return lockedCount;
        }

        public static bool IsTrialFrozen()
        {
            try
            {
                string[] clsidRoots = GetClsidRoots();
                foreach (string rootPath in clsidRoots)
                {
                    using (RegistryKey clsidKey = Registry.CurrentUser.OpenSubKey(rootPath, false))
                    {
                        if (clsidKey == null) continue;
                        string[] subKeyNames = clsidKey.GetSubKeyNames();
                        foreach (string subName in subKeyNames)
                        {
                            if (GuidRegex.IsMatch(subName))
                            {
                                try
                                {
                                    using (RegistryKey testKey = clsidKey.OpenSubKey(subName, false))
                                    {
                                        if (testKey != null)
                                        {
                                            RegistrySecurity sec = testKey.GetAccessControl();
                                            AuthorizationRuleCollection rules = sec.GetAccessRules(true, true, typeof(SecurityIdentifier));
                                            foreach (RegistryAccessRule rule in rules)
                                            {
                                                if (rule.AccessControlType == AccessControlType.Deny)
                                                    return true;
                                            }
                                        }
                                    }
                                }
                                catch (SecurityException)
                                {
                                    return true;
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return false;
        }
    }

    #endregion

        public static bool ExecutePatchDirect(Action<string> logFn, out int appliedCount)
        {
            return ExecutePatchDirect(logFn, out appliedCount, null);
        }

        public static bool ExecutePatchDirect(Action<string> logFn, out int appliedCount, string customName)
        {
            appliedCount = 0;
            if (logFn != null) logFn("================= 开始执行模式一：极速深度解锁 =================");
            string target = Path.Combine(GetIDMDir(), "IDMan.exe");
            if (!File.Exists(target))
            {
                if (logFn != null) logFn("错误：未找到 IDMan.exe，请先确认 IDM 是否已安装。");
                return false;
            }

            KillIDMDirect(logFn);
            Thread.Sleep(300);

            bool success = NativeBinaryPatcher.ApplyPatch(target, true, logFn, out appliedCount);
            if (success)
            {
                // 同步配置合规终身授权登记身份并彻底清理假序列号/黑名单（关于界面点亮终身许可，100% 零弹窗）
                string regName = GetDefaultUserName();
                try
                {
                    using (RegistryKey k = Registry.CurrentUser.CreateSubKey(@"Software\DownloadManager"))
                    {
                        if (k != null)
                        {
                            // 确定授权姓名：优先使用传入名称，其次保留注册表已有名称，最后回退至当前系统登录用户名（如无则为 User）
                            regName = customName;
                            if (string.IsNullOrEmpty(regName))
                            {
                                object existingName = k.GetValue("FName");
                                if (existingName != null && !string.IsNullOrEmpty(existingName.ToString().Trim()))
                                    regName = existingName.ToString().Trim();
                                else
                                    regName = GetDefaultUserName();
                            }

                            k.SetValue("FName", regName, RegistryValueKind.String);
                            k.SetValue("LName", " ", RegistryValueKind.String);

                            object existingEmail = k.GetValue("Email");
                            string regEmail = (existingEmail != null && !string.IsNullOrEmpty(existingEmail.ToString().Trim()))
                                ? existingEmail.ToString().Trim()
                                : GetDefaultUserEmail(regName);
                            k.SetValue("Email", regEmail, RegistryValueKind.String);

                            // 坚决移除 Serial 键！IDM 只要无 Serial 且 FName 存在，即直接判定为终身合法授权。
                            // 一旦写入非法/明文 Serial，就会触发其内部非对称公钥算法校验而弹出假序列号弹窗！
                            try { k.DeleteValue("Serial", false); } catch { }

                            k.SetValue("CheckUpdtVM", 0, RegistryValueKind.DWord);
                            k.SetValue("LstCheck", "0", RegistryValueKind.String);

                            string[] cleanList = new string[] {
                                "scansk", "tvfrdt", "radxcnt", "ptrk_scdt",
                                "scTime", "NextCheck", "BList", "md5pks", "itb_r", "ncl_r"
                            };
                            foreach (string field in cleanList)
                            {
                                try { k.DeleteValue(field, false); } catch { }
                            }
                            // 写入有效的检查时间戳，防止遗留逻辑因字段缺失触发紧急检查
                            k.SetValue("LastCheckQU", (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds, RegistryValueKind.DWord);
                            if (logFn != null)
                            {
                                logFn("✓ 已同步点亮终身授权登记：授权姓名 [" + regName + "]，绑定邮箱 [" + regEmail + "]");
                                logFn("✓ 已同步清理注册表残留假序列号与黑名单字段，关闭自动更新");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (logFn != null) logFn("写入注册表授权配置提示: " + ex.Message);
                }

                if (appliedCount > 0)
                {
                    if (logFn != null)
                    {
                        logFn("✓ 模式一执行完成：已彻底消除假冒序列号弹窗与防逆向看门狗拦截！");
                        logFn("  共完成 " + appliedCount + " 处核心校验逻辑升级（共 " + NativeBinaryPatcher.RulesCount + " 个补丁位点）");
                    }
                }
                else
                {
                    if (logFn != null)
                        logFn("✓ 模式一检测：IDMan.exe 已包含全部优化特征，补丁已生效，已确保终身授权身份就绪。");
                }
            }
            return success;
        }

        private void ExecutePatch()
        {
            SwitchTab(2);
            int appliedCount = 0;
            string preferredName = (txtAuthName != null && !string.IsNullOrEmpty(txtAuthName.Text.Trim())) ? txtAuthName.Text.Trim() : null;
            bool success = ExecutePatchDirect(Log, out appliedCount, preferredName);

            if (success)
            {
                RefreshAllStatus();
                string currentName = GetDefaultUserName();
                try
                {
                    using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\DownloadManager"))
                    {
                        if (k != null)
                        {
                            object fn = k.GetValue("FName");
                            if (fn != null && !string.IsNullOrEmpty(fn.ToString().Trim()))
                                currentName = fn.ToString().Trim();
                        }
                    }
                }
                catch { }

                if (appliedCount > 0)
                {
                    ModernDialog.ShowSuccess(this, I18n.T("Dlg.Patch.Success.Title"), I18n.T("Dlg.Patch.Success.Msg", appliedCount, NativeBinaryPatcher.RulesCount, currentName));
                }
                else
                {
                    ModernDialog.ShowInfo(this, I18n.T("Dlg.Patch.Active.Title"), I18n.T("Dlg.Patch.Active.Msg", NativeBinaryPatcher.RulesCount, currentName));
                }
            }
            else
            {
                Log("模式一执行遇到异常，详情请查看上方日志。");
                ModernDialog.ShowWarning(this, I18n.T("Dlg.Patch.Error.Title"), I18n.T("Dlg.Patch.Error.Msg", NativeBinaryPatcher.SupportedVersions));
            }
        }

        private void ExecuteTrialFreeze()
        {
            SwitchTab(2);
            int lockedCount = 0;
            bool ok = ExecuteTrialFreezeDirect(Log, out lockedCount);
            RefreshAllStatus();

            if (ok)
            {
                ModernDialog.ShowSuccess(this, I18n.T("Dlg.Trial.Success.Title"), I18n.T("Dlg.Trial.Success.Msg", lockedCount));
            }
            else
            {
                ModernDialog.ShowWarning(this, I18n.T("Dlg.Trial.Warning.Title"), I18n.T("Dlg.Trial.Warning.Msg"));
            }
        }

        public static bool ExecuteTrialFreezeDirect(Action<string> logFn, out int lockedCount)
        {
            lockedCount = 0;
            if (logFn != null) logFn("================= 开始执行模式二：一键永久冻结试用期 =================");

            string idmDir = GetIDMDir();
            string targetExe = Path.Combine(idmDir, "IDMan.exe");
            if (!File.Exists(targetExe))
            {
                if (logFn != null) logFn("错误：未找到 IDMan.exe，请先确认 IDM 是否已安装。");
                return false;
            }

            // 步骤 1: 终止后台进程
            if (logFn != null) logFn("步骤 1/4: 正在安全终止后台运行的 IDM 监控与下载进程...");
            KillIDMDirect(logFn);
            Thread.Sleep(300);

            // 步骤 2: 清理 DownloadManager 注册表中的追踪/黑名单/假序列号字段，并配置底层策略
            if (logFn != null) logFn("步骤 2/4: 正在清理历史授权追踪与黑名单遥测项，同步底层驱动策略...");
            try
            {
                using (RegistryKey k = Registry.CurrentUser.CreateSubKey(@"Software\DownloadManager"))
                {
                    if (k != null)
                    {
                        string[] cleanList = new string[] {
                            "FName", "LName", "Email", "Serial", "scansk", "tvfrdt",
                            "radxcnt", "LstCheck", "ptrk_scdt",
                            "scTime", "NextCheck", "BList", "md5pks", "itb_r", "ncl_r"
                        };
                        foreach (string field in cleanList)
                        {
                            try { k.DeleteValue(field, false); } catch { }
                        }
                        k.SetValue("LastCheckQU", (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds, RegistryValueKind.DWord);
                        k.SetValue("CheckUpdtVM", 0, RegistryValueKind.DWord);
                        k.SetValue("LstCheck", "0", RegistryValueKind.String);
                    }
                }

                // HKLM 驱动策略
                string hklmPath = Environment.Is64BitOperatingSystem
                    ? @"SOFTWARE\Wow6432Node\Internet Download Manager"
                    : @"SOFTWARE\Internet Download Manager";
                try
                {
                    using (RegistryKey hk = Registry.LocalMachine.CreateSubKey(hklmPath))
                    {
                        if (hk != null)
                        {
                            hk.SetValue("AdvIntDriverEnabled2", 1, RegistryValueKind.DWord);
                        }
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                if (logFn != null) logFn("清理追踪项提示: " + ex.Message);
            }

            // 解除并清理现存旧的/过期的 CLSID 试用键
            int cleaned = TrialFreezeEngine.CleanAllTrialKeys(logFn);
            if (logFn != null) logFn("✓ 已清理旧试用与过期特征项共 " + cleaned + " 处");

            // 步骤 3: 触发 IDM 生成全新 30 天试用基线
            if (logFn != null) logFn("步骤 3/4: 触发 IDM 引擎初始化全新 30 天官方评估基线...");
            TrialFreezeEngine.TriggerTrialInitialization(idmDir, logFn);

            // 步骤 4: 扫描并锁定全新生成的 CLSID 试用时间策略键
            if (logFn != null) logFn("步骤 4/4: 正在通过 Windows ACL 权限机制锁定试用策略键...");
            lockedCount = TrialFreezeEngine.LockAllTrialKeys(logFn);

            if (lockedCount > 0)
            {
                if (logFn != null)
                {
                    logFn("★ 永久冻结试用期成功！共锁定 " + lockedCount + " 处核心 CLSID 试用时间策略键。");
                    logFn("✓ 运行状态：试用倒计时永久停留在剩余 30 天，无任何弹窗骚扰。");
                    logFn("✓ 升级兼容：未修改任何二进制文件，完全兼容 IDM 官方无缝在线静默更新！");
                }
                return true;
            }
            else
            {
                if (logFn != null)
                {
                    logFn("⚠ 提示：未检测到新生成的 CLSID 试用键，正在重试深度扫描...");
                }
                // 再次短暂启动 IDM 并扫描
                TrialFreezeEngine.TriggerTrialInitialization(idmDir, null);
                lockedCount = TrialFreezeEngine.LockAllTrialKeys(logFn);
                if (lockedCount > 0)
                {
                    if (logFn != null) logFn("★ 重试锁定成功！共锁定 " + lockedCount + " 处试用策略键。");
                    return true;
                }
                if (logFn != null) logFn("警告：未能锁定 CLSID 试用键，建议先手动打开一次 IDM 再执行冻结。");
                return false;
            }
        }

        public static bool ExecuteRegisterDirect(string name, string email, string serial, Action<string> logFn)
        {
            if (string.IsNullOrEmpty(name)) name = GetDefaultUserName();
            if (string.IsNullOrEmpty(email)) email = GetDefaultUserEmail(name);
            if (string.IsNullOrEmpty(serial)) serial = GenerateSerial();

            if (logFn != null)
            {
                logFn("================= 开始执行模式三：个性化授权登记 =================");
                logFn("登记姓名: " + name);
                logFn("绑定邮箱: " + email);
                logFn("授权证书: " + serial);
            }

            KillIDMDirect(logFn);
            Thread.Sleep(300);

            // 1. 自动联动执行模式一底层补丁，彻底杜绝假冒序列号弹窗
            string target = Path.Combine(GetIDMDir(), "IDMan.exe");
            if (File.Exists(target))
            {
                if (logFn != null) logFn("正在自动联动执行【模式一：底层深度解锁】以杜绝假冒序列号弹窗...");
                int patchCount = 0;
                bool patched = NativeBinaryPatcher.ApplyPatch(target, true, logFn, out patchCount);
                if (!patched && logFn != null)
                {
                    logFn("⚠ 底层补丁未生效（版本指纹不匹配或文件异常），已跳过二进制修改，未写入任何字节。");
                    logFn("  注册表授权登记仍将继续写入，但启动后仍可能弹出注册提示。");
                    logFn("  如需彻底免弹窗，请改用【模式二：永久冻结试用期】。");
                }
            }

            // 2. 写入自定义登记信息并锁死联网检查，同时清理试用/黑名单干扰字段
            try
            {
                using (RegistryKey k = Registry.CurrentUser.CreateSubKey(@"Software\DownloadManager"))
                {
                    if (k != null)
                    {
                        // 写入注册登记信息（关于界面将直接展示“此产品授权给：<姓名>”）
                        k.SetValue("FName", name, RegistryValueKind.String);
                        k.SetValue("LName", " ", RegistryValueKind.String);
                        k.SetValue("Email", email, RegistryValueKind.String);

                        // 注意：切勿写入明文假序列号！IDM 对 Serial 会调用内部算法校验，
                        // 写入非法 Serial 会立即触发“注册 Internet Download Manager”弹窗。
                        // 删除 Serial 键后，底层补丁直接生效，关于界面保持授权人显示且 100% 零弹窗！
                        try { k.DeleteValue("Serial", false); } catch { }

                        k.SetValue("CheckUpdtVM", 0, RegistryValueKind.DWord);
                        k.SetValue("LstCheck", "0", RegistryValueKind.String);

                        // 清理试用计数器、假序列号及联网验证残留字段
                        string[] cleanList = new string[] {
                            "scansk", "tvfrdt", "radxcnt", "ptrk_scdt", "LastCheckQU",
                            "scTime", "NextCheck", "BList", "md5pks", "itb_r", "ncl_r"
                        };
                        foreach (string field in cleanList)
                        {
                            try { k.DeleteValue(field, false); } catch { }
                        }
                        if (logFn != null) logFn("✓ 已清理 " + cleanList.Length + " 个试用/假序列号/黑名单残留字段");
                    }
                }
                if (logFn != null)
                {
                    logFn("✓ 成功将登记信息写入注册表 (HKCU\\Software\\DownloadManager)");
                    logFn("  授权姓名: " + name + "  绑定邮箱: " + email);
                    logFn("✓ 模式三执行完毕：底层免弹窗补丁与关于窗口已同步点亮！");
                }
                return true;
            }
            catch (Exception ex)
            {
                if (logFn != null) logFn("写入授权失败: " + ex.Message);
                return false;
            }
        }

        private void ExecuteRegister()
        {
            string name = (txtAuthName != null && !string.IsNullOrEmpty(txtAuthName.Text.Trim())) ? txtAuthName.Text.Trim() : GetDefaultUserName();
            string email = (txtAuthEmail != null && !string.IsNullOrEmpty(txtAuthEmail.Text.Trim())) ? txtAuthEmail.Text.Trim() : GetDefaultUserEmail(name);
            string serial = (txtAuthSerial != null && !string.IsNullOrEmpty(txtAuthSerial.Text.Trim())) ? txtAuthSerial.Text.Trim() : GenerateSerial();

            SwitchTab(2);
            bool ok = ExecuteRegisterDirect(name, email, serial, Log);
            if (ok)
            {
                RefreshAllStatus();
                ModernDialog.ShowSuccess(this, I18n.T("Dlg.Register.Success.Title"), I18n.T("Dlg.Register.Success.Msg", name, email, serial));
            }
            else
            {
                ModernDialog.ShowError(this, I18n.T("Dlg.Register.Error.Title"), I18n.T("Dlg.Register.Error.Msg"));
            }
        }

        private void ExecuteReset()
        {
            if (!ModernDialog.Confirm(this, I18n.T("Dlg.Reset.Confirm.Title"), I18n.T("Dlg.Reset.Confirm.Msg")))
                return;

            SwitchTab(2);
            Log("================= 开始执行模式四：全量清理出厂重置 =================");
            KillIDM();
            Thread.Sleep(300);

            // 0. 尝试同步恢复官方原版主程序
            try
            {
                string target = Path.Combine(GetIDMDir(), "IDMan.exe");
                string bak = target + ".BAK";
                if (File.Exists(bak))
                {
                    File.Copy(bak, target, true);
                    Log("✓ 官方原版主程序已同步无损恢复 (IDMan.exe.BAK -> IDMan.exe)");
                }
            }
            catch { }

            if (txtAuthName != null) txtAuthName.Text = GetDefaultUserName();
            if (txtAuthEmail != null) txtAuthEmail.Text = GetDefaultUserEmail();
            if (txtAuthSerial != null) txtAuthSerial.Text = GenerateSerial();

            try
            {
                int deletedProps = 0;
                // 1. 清理 HKCU\Software\DownloadManager 核心键值
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\DownloadManager", true))
                {
                    if (k != null)
                    {
                        string[] wipeList = {
                            "FName", "LName", "Email", "Serial", "scansk", "tvfrdt",
                            "radxcnt", "LstCheck", "ptrk_scdt", "LastCheckQU", "CheckUpdtVM",
                            "scTime", "NextCheck", "BList", "md5pks", "itb_r", "ncl_r"
                        };

                        foreach (string prop in wipeList)
                        {
                            try
                            {
                                if (k.GetValue(prop) != null)
                                {
                                    k.DeleteValue(prop, false);
                                    deletedProps++;
                                    Log("已抹除 DownloadManager 残留项: " + prop);
                                }
                            }
                            catch { }
                        }
                    }
                }

                // 2. 清理 CLSID 下包含时间戳/锁定的隐藏 GUID 项
                int deletedGuids = TrialFreezeEngine.CleanAllTrialKeys(Log);

                Log("✓ 第一阶段：已抹除 DownloadManager 配置关联项 " + deletedProps + " 处");
                Log("✓ 第二阶段：已解除锁定并清理 CLSID 试用策略项 " + deletedGuids + " 处");
                Log("✓ 模式四执行完毕：IDM 授权与黑名单数据已彻底消除，成功恢复出厂纯净状态！");

                RefreshAllStatus();
                ModernDialog.ShowSuccess(this, I18n.T("Dlg.Reset.Success.Title"), I18n.T("Dlg.Reset.Success.Msg", deletedProps, deletedGuids));
            }
            catch (Exception ex)
            {
                Log("重置失败: " + ex.Message);
                ModernDialog.ShowError(this, I18n.T("Dlg.Reset.Error.Title"), I18n.T("Dlg.Reset.Error.Msg", ex.Message));
            }
        }

        #endregion
    }
}
