import { FormEvent, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getUserById, updateUser, getApiErrorMessage } from '../api/usersApi';
import { useAuthState } from '../auth/useAuthState';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { User } from '../types/user';

interface FormValues {
  name: string;
  age: string;
  city: string;
  state: string;
  pincode: string;
}

type ValidationErrors = Partial<Record<keyof FormValues, string>>;

function validate(values: FormValues): ValidationErrors {
  const errors: ValidationErrors = {};

  if (!values.name.trim()) {
    errors.name = 'Name is required.';
  } else if (values.name.trim().length < 2 || values.name.trim().length > 100) {
    errors.name = 'Name must be between 2 and 100 characters.';
  }

  const parsedAge = Number(values.age);
  if (values.age.trim().length === 0 || Number.isNaN(parsedAge)) {
    errors.age = 'Age is required.';
  } else if (parsedAge < 0 || parsedAge > 120) {
    errors.age = 'Age must be between 0 and 120.';
  }

  if (!values.city.trim()) {
    errors.city = 'City is required.';
  }

  if (!values.state.trim()) {
    errors.state = 'State is required.';
  }

  if (!values.pincode.trim()) {
    errors.pincode = 'Pincode is required.';
  } else if (values.pincode.trim().length < 4 || values.pincode.trim().length > 10) {
    errors.pincode = 'Pincode must be between 4 and 10 characters.';
  }

  return errors;
}

export function EditUserPage(): JSX.Element {
  const [user, setUser] = useState<User | null>(null);
  const [values, setValues] = useState<FormValues>({
    name: '',
    age: '',
    city: '',
    state: '',
    pincode: ''
  });
  const [errors, setErrors] = useState<ValidationErrors>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { id } = useParams<{ id: string }>();
  const auth = useAuthState();
  const navigate = useNavigate();

  const hasErrors = useMemo(() => Object.keys(errors).length > 0, [errors]);

  useEffect(() => {
    if (!id) {
      navigate('/users', { replace: true });
      return;
    }

    let isMounted = true;

    async function loadUser(): Promise<void> {
      try {
        setIsLoading(true);
        const fetchedUser = await getUserById(id);
        if (isMounted) {
          setUser(fetchedUser);
          setValues({
            name: fetchedUser.name,
            age: fetchedUser.age.toString(),
            city: fetchedUser.city,
            state: fetchedUser.state,
            pincode: fetchedUser.pincode
          });
        }
      } catch (requestError) {
        if (isMounted) {
          if (requestError instanceof Error && 'response' in requestError && (requestError as any).response?.status === 401) {
            auth.logout();
            navigate('/login', { replace: true, state: { from: `/users/${id}/edit` } });
            return;
          }
          if (requestError instanceof Error && 'response' in requestError && (requestError as any).response?.status === 404) {
            navigate('/users', { replace: true, state: { toastMessage: 'User not found.' } });
            return;
          }
          setFormError(getApiErrorMessage(requestError));
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    void loadUser();

    return () => {
      isMounted = false;
    };
  }, [id, auth, navigate]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();

    if (!id) {
      return;
    }

    const validationErrors = validate(values);
    setErrors(validationErrors);
    setFormError(null);

    if (Object.keys(validationErrors).length > 0) {
      return;
    }

    try {
      setIsSubmitting(true);

      await updateUser(id, {
        name: values.name.trim(),
        age: Number(values.age),
        city: values.city.trim(),
        state: values.state.trim(),
        pincode: values.pincode.trim()
      });

      navigate('/users', {
        state: {
          toastMessage: 'User updated successfully.'
        }
      });
    } catch (requestError) {
      setFormError(getApiErrorMessage(requestError));
    } finally {
      setIsSubmitting(false);
    }
  }

  if (isLoading) {
    return (
      <section className="page-card">
        <LoadingSpinner label="Loading user..." />
      </section>
    );
  }

  if (formError && !user) {
    return (
      <section className="page-card">
        <p className="status-message error">Failed to load user: {formError}</p>
      </section>
    );
  }

  return (
    <section className="page-card">
      <header className="page-header">
        <h1>Edit User</h1>
        <p>Update user information.</p>
      </header>

      <form className="form-grid" onSubmit={(event) => void handleSubmit(event)} noValidate>
        <label>
          Name
          <input
            type="text"
            value={values.name}
            onChange={(event) => setValues((current) => ({ ...current, name: event.target.value }))}
            placeholder="Enter name"
          />
          {errors.name ? <span className="input-error">{errors.name}</span> : null}
        </label>

        <label>
          Age
          <input
            type="number"
            value={values.age}
            onChange={(event) => setValues((current) => ({ ...current, age: event.target.value }))}
            placeholder="Enter age"
            min={0}
            max={120}
          />
          {errors.age ? <span className="input-error">{errors.age}</span> : null}
        </label>

        <label>
          City
          <input
            type="text"
            value={values.city}
            onChange={(event) => setValues((current) => ({ ...current, city: event.target.value }))}
            placeholder="Enter city"
          />
          {errors.city ? <span className="input-error">{errors.city}</span> : null}
        </label>

        <label>
          State
          <input
            type="text"
            value={values.state}
            onChange={(event) => setValues((current) => ({ ...current, state: event.target.value }))}
            placeholder="Enter state"
          />
          {errors.state ? <span className="input-error">{errors.state}</span> : null}
        </label>

        <label>
          Pincode
          <input
            type="text"
            value={values.pincode}
            onChange={(event) => setValues((current) => ({ ...current, pincode: event.target.value }))}
            placeholder="Enter pincode"
          />
          {errors.pincode ? <span className="input-error">{errors.pincode}</span> : null}
        </label>

        {formError ? <p className="status-message error">Failed to update user: {formError}</p> : null}

        <div className="button-row">
          <button className="primary-button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : 'Update User'}
          </button>
          <button
            type="button"
            className="secondary-button"
            onClick={() => navigate('/users')}
            disabled={isSubmitting}
          >
            Cancel
          </button>
          {hasErrors ? <span className="hint-text">Please correct the errors.</span> : null}
        </div>
      </form>
    </section>
  );
}