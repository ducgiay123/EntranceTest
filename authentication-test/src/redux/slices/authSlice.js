import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import axios from "axios";

// Thunk for login
export const loginUser = createAsyncThunk(
  "auth/loginUser",
  async ({ email, password }, thunkAPI) => {
    try {
      const res = await axios.post("http://localhost:5000/api/v1/auth/login", {
        email,
        password,
      });
      const { token, refreshToken, user } = res.data;

      // Save to session storage
      sessionStorage.setItem("token", token);
      sessionStorage.setItem("refreshToken", refreshToken);
      sessionStorage.setItem("user", JSON.stringify(user)); // Save user to session

      return { token, refreshToken, user };
    } catch (err) {
      return thunkAPI.rejectWithValue(
        err.response?.data?.message || "Login failed"
      );
    }
  }
);

// Thunk for logout
export const logoutUser = createAsyncThunk(
  "auth/logoutUser",
  async (_, thunkAPI) => {
    try {
      const token = sessionStorage.getItem("token");
      if (!token) {
        return thunkAPI.rejectWithValue("No token found");
      }

      // Send POST request with Authorization header
      await axios.post(
        "http://localhost:5000/api/v1/auth/logout",
        {}, // Empty body since logout doesn't need data
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      // Clear session storage
      sessionStorage.clear();
      return true;
    } catch (err) {
      console.error("Logout error:", err.response?.data);
      return thunkAPI.rejectWithValue(
        err.response?.data?.message || "Logout failed"
      );
    }
  }
);
export const fetchUser = createAsyncThunk(
  "auth/fetchUser",
  async (_, thunkAPI) => {
    try {
      const token = sessionStorage.getItem("token");
      if (!token) {
        return thunkAPI.rejectWithValue("No token found");
      }

      const res = await axios.get("http://localhost:5000/api/v1/auth/user", {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      const user = res.data;
      sessionStorage.setItem("user", JSON.stringify(user)); // Persist user
      return user;
    } catch (err) {
      return thunkAPI.rejectWithValue(
        err.response?.data?.message || "Failed to fetch user"
      );
    }
  }
);
const authSlice = createSlice({
  name: "auth",
  initialState: {
    user: null,
    token: sessionStorage.getItem("token") || null,
    loading: false,
    error: null,
  },
  reducers: {
    logout(state) {
      state.user = null;
      state.token = null;
      sessionStorage.clear();
    },
    setUserFromSession(state, action) {
      state.user = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      // Login
      .addCase(loginUser.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(loginUser.fulfilled, (state, action) => {
        state.loading = false;
        state.user = action.payload.user;
        state.token = action.payload.token;
      })
      .addCase(loginUser.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })

      // Logout
      .addCase(logoutUser.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(logoutUser.fulfilled, (state) => {
        state.loading = false;
        state.user = null;
        state.token = null;
      })
      .addCase(logoutUser.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { logout, setUserFromSession } = authSlice.actions;
export default authSlice.reducer;
