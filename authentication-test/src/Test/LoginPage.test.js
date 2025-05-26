/* eslint-disable testing-library/no-wait-for-multiple-assertions */
import React from "react";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { Provider } from "react-redux";
import { configureStore } from "@reduxjs/toolkit";
import { MemoryRouter } from "react-router-dom";
import LoginPage from "./LoginPage";
import { loginUser } from "../redux/slices/authSlice";

// Mock dependencies
jest.mock("axios");
jest.mock("../redux/slices/authSlice", () => ({
  loginUser: jest.fn(),
}));

// Mock useNavigate from react-router-dom
const mockNavigate = jest.fn();
jest.mock("react-router-dom", () => ({
  ...jest.requireActual("react-router-dom"),
  useNavigate: () => mockNavigate,
  Link: ({ children, to }) => <a href={to}>{children}</a>,
}));

// Create a mock Redux store
const mockStore = configureStore({
  reducer: {
    auth: (state = { loading: false, error: null, token: null }, action) =>
      state,
  },
});

describe("LoginPage Component", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockStore.dispatch = jest.fn();
  });

  // Test 1: Renders the login form correctly
  test("renders login form with email, password inputs, and login button", () => {
    render(
      <Provider store={mockStore}>
        <MemoryRouter>
          <LoginPage />
        </MemoryRouter>
      </Provider>
    );

    expect(screen.getByLabelText(/Email\*/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Password\*/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /Login/i })).toBeInTheDocument();
    expect(screen.getByText(/Create an account/i)).toBeInTheDocument();
  });

  // Test 2: Updates email and password inputs on user input
  test("updates email and password inputs on user input", () => {
    render(
      <Provider store={mockStore}>
        <MemoryRouter>
          <LoginPage />
        </MemoryRouter>
      </Provider>
    );

    const emailInput = screen.getByLabelText(/Email\*/i);
    const passwordInput = screen.getByLabelText(/Password\*/i);

    fireEvent.change(emailInput, { target: { value: "test@example.com" } });
    fireEvent.change(passwordInput, { target: { value: "password123" } });

    expect(emailInput).toHaveValue("test@example.com");
    expect(passwordInput).toHaveValue("password123");
  });

  // Test 3: Displays validation errors for invalid email and empty password
  test("displays validation errors for invalid email and empty password", async () => {
    render(
      <Provider store={mockStore}>
        <MemoryRouter>
          <LoginPage />
        </MemoryRouter>
      </Provider>
    );

    const emailInput = screen.getByLabelText(/Email\*/i);
    const passwordInput = screen.getByLabelText(/Password\*/i);
    const loginButton = screen.getByRole("button", { name: /Login/i });

    // Enter invalid email and no password
    fireEvent.change(emailInput, { target: { value: "invalid-email" } });
    fireEvent.change(passwordInput, { target: { value: "" } });
    fireEvent.click(loginButton);

    await waitFor(() => {
      expect(
        screen.getByText(/Email is required and must be valid./i)
      ).toBeInTheDocument();
      expect(screen.getByText(/Password is required./i)).toBeInTheDocument();
    });
  });

  // Test 4: Dispatches loginUser action on valid form submission
  test("dispatches loginUser action on valid form submission", async () => {
    loginUser.mockReturnValue({
      type: "auth/loginUser/fulfilled",
      meta: { requestStatus: "fulfilled" },
    });

    render(
      <Provider store={mockStore}>
        <MemoryRouter>
          <LoginPage />
        </MemoryRouter>
      </Provider>
    );

    const emailInput = screen.getByLabelText(/Email\*/i);
    const passwordInput = screen.getByLabelText(/Password\*/i);
    const loginButton = screen.getByRole("button", { name: /Login/i });

    fireEvent.change(emailInput, { target: { value: "test@example.com" } });
    fireEvent.change(passwordInput, { target: { value: "password123" } });
    fireEvent.click(loginButton);

    await waitFor(() => {
      expect(loginUser).toHaveBeenCalledWith({
        email: "test@example.com",
        password: "password123",
      });
      expect(mockNavigate).toHaveBeenCalledWith("/dashboard");
    });
  });

  // Test 5: Shows alert on login failure
  test("shows alert on login failure", async () => {
    loginUser.mockReturnValue({
      type: "auth/loginUser/rejected",
      meta: { requestStatus: "rejected" },
    });
    jest.spyOn(window, "alert").mockImplementation(() => {});

    render(
      <Provider store={mockStore}>
        <MemoryRouter>
          <LoginPage />
        </MemoryRouter>
      </Provider>
    );

    const emailInput = screen.getByLabelText(/Email\*/i);
    const passwordInput = screen.getByLabelText(/Password\*/i);
    const loginButton = screen.getByRole("button", { name: /Login/i });

    fireEvent.change(emailInput, { target: { value: "test@example.com" } });
    fireEvent.change(passwordInput, { target: { value: "password123" } });
    fireEvent.click(loginButton);

    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith(
        "Login failed. Please check your email or password and try again."
      );
    });
  });

  // Test 6: Navigates to dashboard if token exists
  test("navigates to dashboard if token exists", () => {
    const storeWithToken = configureStore({
      reducer: {
        auth: (
          state = { loading: false, error: null, token: "some-token" },
          action
        ) => state,
      },
    });

    render(
      <Provider store={storeWithToken}>
        <MemoryRouter>এ</MemoryRouter>
      </Provider>
    );

    expect(mockNavigate).toHaveBeenCalledWith("/dashboard");
  });
});
