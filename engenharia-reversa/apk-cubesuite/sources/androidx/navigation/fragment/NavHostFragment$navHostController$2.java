package androidx.navigation.fragment;

import android.content.Context;
import android.os.Bundle;
import androidx.core.os.BundleKt;
import androidx.lifecycle.ViewModelStore;
import androidx.navigation.NavHostController;
import androidx.savedstate.SavedStateRegistry;
import kotlin.Metadata;
import kotlin.TuplesKt;
import kotlin.jvm.functions.Function0;
import kotlin.jvm.internal.Intrinsics;
import kotlin.jvm.internal.Lambda;

/* JADX INFO: compiled from: NavHostFragment.kt */
/* JADX INFO: loaded from: classes.dex */
@Metadata(d1 = {"\u0000\b\n\u0000\n\u0002\u0018\u0002\n\u0000\u0010\u0000\u001a\u00020\u0001H\n¢\u0006\u0002\b\u0002"}, d2 = {"<anonymous>", "Landroidx/navigation/NavHostController;", "invoke"}, k = 3, mv = {1, 8, 0}, xi = 48)
final class NavHostFragment$navHostController$2 extends Lambda implements Function0<NavHostController> {
    final /* synthetic */ NavHostFragment this$0;

    /* JADX WARN: 'super' call moved to the top of the method (can break code semantics) */
    NavHostFragment$navHostController$2(NavHostFragment navHostFragment) {
        super(0);
        this.this$0 = navHostFragment;
    }

    /* JADX WARN: Can't rename method to resolve collision */
    @Override // kotlin.jvm.functions.Function0
    public final NavHostController invoke() {
        Context context = this.this$0.getContext();
        if (context == null) {
            throw new IllegalStateException("NavController cannot be created before the fragment is attached".toString());
        }
        Intrinsics.checkNotNullExpressionValue(context, "checkNotNull(context) {\n…nt is attached\"\n        }");
        final NavHostController navHostController = new NavHostController(context);
        final NavHostFragment navHostFragment = this.this$0;
        navHostController.setLifecycleOwner(navHostFragment);
        ViewModelStore viewModelStore = navHostFragment.getViewModelStore();
        Intrinsics.checkNotNullExpressionValue(viewModelStore, "viewModelStore");
        navHostController.setViewModelStore(viewModelStore);
        navHostFragment.onCreateNavHostController(navHostController);
        Bundle bundleConsumeRestoredStateForKey = navHostFragment.getSavedStateRegistry().consumeRestoredStateForKey("android-support-nav:fragment:navControllerState");
        if (bundleConsumeRestoredStateForKey != null) {
            navHostController.restoreState(bundleConsumeRestoredStateForKey);
        }
        navHostFragment.getSavedStateRegistry().registerSavedStateProvider("android-support-nav:fragment:navControllerState", new SavedStateRegistry.SavedStateProvider() { // from class: androidx.navigation.fragment.NavHostFragment$navHostController$2$$ExternalSyntheticLambda0
            @Override // androidx.savedstate.SavedStateRegistry.SavedStateProvider
            public final Bundle saveState() {
                return NavHostFragment$navHostController$2.invoke$lambda$5$lambda$2(navHostController);
            }
        });
        Bundle bundleConsumeRestoredStateForKey2 = navHostFragment.getSavedStateRegistry().consumeRestoredStateForKey(NavHostFragment.KEY_GRAPH_ID);
        if (bundleConsumeRestoredStateForKey2 != null) {
            navHostFragment.graphId = bundleConsumeRestoredStateForKey2.getInt(NavHostFragment.KEY_GRAPH_ID);
        }
        navHostFragment.getSavedStateRegistry().registerSavedStateProvider(NavHostFragment.KEY_GRAPH_ID, new SavedStateRegistry.SavedStateProvider() { // from class: androidx.navigation.fragment.NavHostFragment$navHostController$2$$ExternalSyntheticLambda1
            @Override // androidx.savedstate.SavedStateRegistry.SavedStateProvider
            public final Bundle saveState() {
                return NavHostFragment$navHostController$2.invoke$lambda$5$lambda$4(navHostFragment);
            }
        });
        if (navHostFragment.graphId != 0) {
            navHostController.setGraph(navHostFragment.graphId);
            return navHostController;
        }
        Bundle arguments = navHostFragment.getArguments();
        int i = arguments != null ? arguments.getInt(NavHostFragment.KEY_GRAPH_ID) : 0;
        Bundle bundle = arguments != null ? arguments.getBundle(NavHostFragment.KEY_START_DESTINATION_ARGS) : null;
        if (i != 0) {
            navHostController.setGraph(i, bundle);
        }
        return navHostController;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public static final Bundle invoke$lambda$5$lambda$2(NavHostController this_apply) {
        Intrinsics.checkNotNullParameter(this_apply, "$this_apply");
        Bundle bundleSaveState = this_apply.saveState();
        if (bundleSaveState != null) {
            return bundleSaveState;
        }
        Bundle EMPTY = Bundle.EMPTY;
        Intrinsics.checkNotNullExpressionValue(EMPTY, "EMPTY");
        return EMPTY;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public static final Bundle invoke$lambda$5$lambda$4(NavHostFragment this$0) {
        Intrinsics.checkNotNullParameter(this$0, "this$0");
        if (this$0.graphId != 0) {
            return BundleKt.bundleOf(TuplesKt.to(NavHostFragment.KEY_GRAPH_ID, Integer.valueOf(this$0.graphId)));
        }
        Bundle bundle = Bundle.EMPTY;
        Intrinsics.checkNotNullExpressionValue(bundle, "{\n                    Bu…e.EMPTY\n                }");
        return bundle;
    }
}
